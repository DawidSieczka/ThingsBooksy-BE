import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { AnimatedBackgroundComponent } from '../../../shared/components/animated-background/animated-background.component';
import { DashboardHeaderComponent } from '../dashboard-header/dashboard-header.component';
import { DashboardWelcomeBarComponent } from '../dashboard-welcome-bar/dashboard-welcome-bar.component';
import { DashboardHistoryPanelComponent } from '../dashboard-history-panel/dashboard-history-panel.component';
import { DashboardAdminPanelComponent } from '../dashboard-admin-panel/dashboard-admin-panel.component';
import { CreateOrEditGroupModalComponent } from '../create-group-modal/create-group-modal.component';
import { HISTORY_ROWS, MEMBER_GROUPS, ADMIN_GROUPS } from '../mock-data';

@Component({
  selector: 'tb-dashboard-page',
  standalone: true,
  imports: [
    AnimatedBackgroundComponent,
    DashboardHeaderComponent,
    DashboardWelcomeBarComponent,
    DashboardHistoryPanelComponent,
    DashboardAdminPanelComponent,
    CreateOrEditGroupModalComponent,
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly displayName = this.authService.displayName;
  readonly initials = this.authService.initials;
  readonly modalOpen = signal(false);

  readonly today = new Date();
  readonly historyRows = HISTORY_ROWS;
  readonly memberGroups = MEMBER_GROUPS;
  readonly adminGroups = ADMIN_GROUPS;

  onCreateGroup(): void {
    this.modalOpen.set(true);
  }

  onCloseModal(): void {
    this.modalOpen.set(false);
  }

  onViewAll(): void {
    // Future: navigate to /bookings — not in scope for this iteration
  }

  onSettings(): void {
    // Future: navigate to /settings — not in scope for this iteration
    console.log('Settings TBD');
  }

  onLogout(): void {
    this.authService.signOut().subscribe(() => {
      void this.router.navigate(['/']);
    });
  }
}
