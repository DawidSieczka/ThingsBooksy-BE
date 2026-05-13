import { Component, computed, inject, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export type TextFieldIcon = 'mail' | 'user' | 'lock';
export type TextFieldType = 'text' | 'email' | 'password';

let nextId = 0;

@Component({
  selector: 'tb-text-field',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './text-field.component.html',
  styleUrl: './text-field.component.scss',
})
export class TextFieldComponent {
  private readonly destroyRef = inject(DestroyRef);

  readonly label = input.required<string>();
  readonly icon = input.required<TextFieldIcon>();
  readonly control = input.required<FormControl<string>>();
  readonly type = input<TextFieldType>('text');
  readonly placeholder = input<string>('');
  readonly autocomplete = input<string>('off');
  readonly errorMessages = input<Record<string, string>>({});

  readonly inputId = `tb-text-field-${nextId++}`;
  private readonly showPassword = signal(false);
  private readonly forceErrorRefresh = signal(0);

  readonly effectiveType = computed(() =>
    this.type() === 'password' && this.showPassword() ? 'text' : this.type(),
  );

  readonly canTogglePassword = computed(() => this.type() === 'password');

  readonly errors = computed(() => {
    this.forceErrorRefresh();
    const ctrl = this.control();
    if (!ctrl.touched || !ctrl.errors) {
      return [];
    }
    const messages = this.errorMessages();
    return Object.keys(ctrl.errors).map(key => messages[key] ?? this.defaultMessage(key));
  });

  readonly hasErrors = computed(() => this.errors().length > 0);

  constructor() {
    // Touch + status changes don't trigger the input signal — force re-evaluation
    // by bumping a tracked counter when the control's status changes.
    queueMicrotask(() => {
      this.control()
        .statusChanges.pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.forceErrorRefresh.update(v => v + 1));
    });
  }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  showingPassword(): boolean {
    return this.showPassword();
  }

  private defaultMessage(key: string): string {
    switch (key) {
      case 'required': return 'This field is required.';
      case 'email': return 'Please enter a valid email address.';
      case 'minlength': return 'Value is too short.';
      default: return 'Invalid value.';
    }
  }
}
