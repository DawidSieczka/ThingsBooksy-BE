import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  effect,
  inject,
  viewChild,
} from '@angular/core';
import { ModalComponent } from '../modal/modal.component';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';

@Component({
  selector: 'tb-confirm-dialog',
  standalone: true,
  imports: [ModalComponent],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  private readonly confirmDialogService = inject(ConfirmDialogService);

  private readonly confirmButtonRef =
    viewChild<ElementRef<HTMLButtonElement>>('confirmBtn');

  readonly state = this.confirmDialogService.state;
  readonly isOpen = computed(() => this.state() !== null);

  constructor() {
    effect(() => {
      const currentState = this.state();
      if (currentState) {
        // Defer focus to the next microtask so the dialog has time to open
        Promise.resolve().then(() => {
          this.confirmButtonRef()?.nativeElement?.focus();
        });
      }
    });
  }

  onConfirm(): void {
    this.confirmDialogService.onConfirm();
  }

  onCancel(): void {
    this.confirmDialogService.onCancel();
  }
}
