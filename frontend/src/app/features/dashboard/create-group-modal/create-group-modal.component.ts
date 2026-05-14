import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import {
  AbstractControl,
  AsyncValidatorFn,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Observable, catchError, first, of, switchMap, timer } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { DashboardService } from '../dashboard.service';

export interface GroupFormValue {
  id: string;
  name: string;
  description: string | null;
}

function groupNameAvailabilityValidator(
  dashboardService: DashboardService,
  getInitialName: () => string | null,
  getMode: () => 'create' | 'edit',
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    const value: string = (control.value ?? '').trim();

    if (!value) {
      return of(null);
    }

    // In edit mode, skip the check when the value equals the original name
    if (getMode() === 'edit' && value === (getInitialName() ?? '').trim()) {
      return of(null);
    }

    return timer(300).pipe(
      switchMap(() => dashboardService.isGroupNameAvailable(value)),
      first(),
      catchError(() => of(null)),
      switchMap(available => {
        if (available === null) return of(null);
        return of(available ? null : ({ taken: true } as ValidationErrors));
      }),
    );
  };
}

@Component({
  selector: 'tb-create-group-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ModalComponent, ReactiveFormsModule],
  templateUrl: './create-group-modal.component.html',
  styleUrl: './create-group-modal.component.scss',
})
export class CreateOrEditGroupModalComponent implements OnInit {
  // --- DI ---
  private readonly fb = inject(FormBuilder);
  private readonly dashboardService = inject(DashboardService);
  private readonly destroyRef = inject(DestroyRef);

  // --- Inputs / Outputs ---
  readonly open = input.required<boolean>();
  readonly mode = input<'create' | 'edit'>('create');
  readonly initialValue = input<{ id: string; name: string; description: string | null } | null>(
    null,
  );

  readonly close = output<void>();
  readonly submitted = output<GroupFormValue>();

  // --- Local state ---
  readonly submitting = signal(false);
  readonly title = computed(() =>
    this.mode() === 'create' ? 'Create new group' : 'Edit group',
  );
  readonly submitLabel = computed(() =>
    this.mode() === 'create' ? 'Create' : 'Save',
  );
  readonly descLength = computed(() => {
    const v = this.form.controls.description.value;
    return v ? v.length : 0;
  });

  // --- Form ---
  readonly form = this.fb.group({
    name: this.fb.control('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(100),
      ],
      asyncValidators: [
        groupNameAvailabilityValidator(
          this.dashboardService,
          () => this.initialValue()?.name ?? null,
          () => this.mode(),
        ),
      ],
      updateOn: 'blur',
    }),
    description: this.fb.control<string | null>(null, {
      validators: [Validators.maxLength(500)],
    }),
  });

  // --- View children ---
  private readonly nameInputRef = viewChild<ElementRef<HTMLInputElement>>('nameInput');

  constructor() {
    // Auto-focus name input when modal opens
    effect(() => {
      if (this.open()) {
        // Small delay to let the dialog finish showing
        setTimeout(() => {
          this.nameInputRef()?.nativeElement?.focus();
        }, 50);
      }
    });

    // Patch form when initialValue changes (edit mode)
    effect(() => {
      const initial = this.initialValue();
      if (initial && this.mode() === 'edit') {
        this.form.patchValue(
          { name: initial.name, description: initial.description },
          { emitEvent: false },
        );
        this.form.markAsPristine();
        this.form.markAsUntouched();
      }
    });
  }

  ngOnInit(): void {
    const initial = this.initialValue();
    if (initial && this.mode() === 'edit') {
      this.form.patchValue(
        { name: initial.name, description: initial.description },
        { emitEvent: false },
      );
      this.form.markAsPristine();
      this.form.markAsUntouched();
    }
  }

  onClose(): void {
    this.close.emit();
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid || this.form.pending || this.submitting()) {
      return;
    }

    const { name, description } = this.form.getRawValue();
    const trimmedName = name.trim();

    this.submitting.set(true);

    try {
      if (this.mode() === 'create') {
        const result = await firstValueFrom(
          this.dashboardService.createGroup({
            name: trimmedName,
            description: description ?? null,
          }),
        );
        this.submitted.emit(result);
      } else {
        const id = this.initialValue()!.id;
        const result = await firstValueFrom(
          this.dashboardService.updateGroup({
            id,
            name: trimmedName,
            description: description ?? null,
          }),
        );
        this.submitted.emit(result);
      }
    } catch {
      // errorInterceptor surfaces toast; let user retry
    } finally {
      this.submitting.set(false);
    }
  }
}
