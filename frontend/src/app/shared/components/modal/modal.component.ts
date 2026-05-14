import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  output,
  viewChild,
} from '@angular/core';
import { trigger, transition, style, animate } from '@angular/animations';
import { IconCloseComponent } from '../icon-close/icon-close.component';

@Component({
  selector: 'tb-modal',
  standalone: true,
  imports: [IconCloseComponent],
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('modalEnter', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.96)' }),
        animate(
          '220ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({ opacity: 1, transform: 'scale(1)' }),
        ),
      ]),
    ]),
  ],
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
