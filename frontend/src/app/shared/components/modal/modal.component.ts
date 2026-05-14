import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  output,
  viewChild,
} from '@angular/core';
import { IconCloseComponent } from '../icon-close/icon-close.component';

@Component({
  selector: 'tb-modal',
  standalone: true,
  imports: [IconCloseComponent],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalComponent {
  private readonly dialogRef =
    viewChild.required<ElementRef<HTMLDialogElement>>('dialogRef');

  readonly open = input.required<boolean>();
  readonly title = input<string>('');

  readonly close = output<void>();

  constructor() {
    effect(() => {
      const isOpen = this.open();
      const el = this.dialogRef()?.nativeElement;
      if (!el) return;
      if (isOpen && !el.open) {
        el.showModal();
      } else if (!isOpen && el.open) {
        el.close();
      }
    });
  }

  onCancel(event: Event): void {
    event.preventDefault();
    this.close.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialogRef().nativeElement) {
      this.close.emit();
    }
  }
}
