import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { animate, style, transition, trigger } from '@angular/animations';
import { IconChevronComponent } from '../icon-chevron/icon-chevron.component';
import { IconLogoutComponent } from '../icon-logout/icon-logout.component';
import { IconSettingsComponent } from '../icon-settings/icon-settings.component';

@Component({
  selector: 'tb-user-menu',
  standalone: true,
  imports: [IconChevronComponent, IconLogoutComponent, IconSettingsComponent],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('fadeDown', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-8px)' }),
        animate(
          '180ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
      transition(':leave', [
        animate(
          '120ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({ opacity: 0, transform: 'translateY(-8px)' }),
        ),
      ]),
    ]),
  ],
})
export class UserMenuComponent {
  private readonly hostRef = inject(ElementRef);

  readonly name = input.required<string>();
  readonly initials = input.required<string>();

  readonly settings = output<void>();
  readonly logout = output<void>();

  readonly isOpen = signal(false);

  toggle(event: Event): void {
    event.stopPropagation();
    this.isOpen.update(v => !v);
  }

  @HostListener('document:keydown.escape')
  onEscKey(): void {
    this.isOpen.set(false);
  }

  onDocumentClick(event: Event): void {
    const target = event.target as Node;
    if (!this.hostRef.nativeElement.contains(target)) {
      this.isOpen.set(false);
    }
  }

  onSettings(): void {
    this.isOpen.set(false);
    this.settings.emit();
  }

  onLogout(): void {
    this.isOpen.set(false);
    this.logout.emit();
  }
}
