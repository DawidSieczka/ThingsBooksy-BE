import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService, Toast } from '../../services/notification.service';
import { IconCloseComponent } from '../icon-close/icon-close.component';

@Component({
  selector: 'tb-toast',
  standalone: true,
  imports: [IconCloseComponent],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastComponent {
  private readonly notificationService = inject(NotificationService);

  readonly toasts = this.notificationService.toasts;

  dismiss(id: string): void {
    this.notificationService.dismiss(id);
  }

  kindLabel(toast: Toast): string {
    const prefix: Record<Toast['kind'], string> = {
      success: 'Success',
      error: 'Error',
      info: 'Info',
    };
    return `${prefix[toast.kind]}: ${toast.message}`;
  }
}
