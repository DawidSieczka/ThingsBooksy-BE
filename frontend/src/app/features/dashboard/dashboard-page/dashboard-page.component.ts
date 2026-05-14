import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { AnimatedBackgroundComponent } from '../../../shared/components/animated-background/animated-background.component';
import { NotificationService } from '../../../shared/services/notification.service';
import { GroupListItemDto, GroupsApiService } from '../../groups/services/groups-api.service';
import { DashboardHeaderComponent } from '../dashboard-header/dashboard-header.component';
import { DashboardWelcomeBarComponent } from '../dashboard-welcome-bar/dashboard-welcome-bar.component';
import { DashboardHistoryPanelComponent } from '../dashboard-history-panel/dashboard-history-panel.component';
import { DashboardAdminPanelComponent } from '../dashboard-admin-panel/dashboard-admin-panel.component';
import { CreateOrEditGroupModalComponent, GroupFormValue } from '../create-group-modal/create-group-modal.component';
import { HISTORY_ROWS } from '../mock-data';

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
export class DashboardPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  private readonly groupsApi = inject(GroupsApiService);

  readonly displayName = this.authService.displayName;
  readonly initials = this.authService.initials;
  readonly modalOpen = signal(false);

  readonly today = new Date();
  readonly historyRows = HISTORY_ROWS;

  private readonly _groups = signal<GroupListItemDto[]>([]);
  readonly groupsLoading = signal(false);

  readonly ownedGroups = computed(() => {
    const me = this.authService.currentUser();
    if (!me) return [];
    return this._groups().filter(g => g.ownerId === me.id);
  });

  readonly memberGroups = computed(() => {
    const me = this.authService.currentUser();
    if (!me) return [];
    return this._groups().filter(g => g.ownerId !== me.id);
  });

  ngOnInit(): void {
    this.loadGroups();
  }

  onCreateGroup(): void {
    this.modalOpen.set(true);
  }

  onCloseModal(): void {
    this.modalOpen.set(false);
  }

  onGroupSubmitted(result: GroupFormValue): void {
    this.modalOpen.set(false);
    this.notifications.success('Group created');
    void this.router.navigate(['/groups', result.id]);
  }

  onViewAll(): void {
    // Future: navigate to /bookings — not in scope for this iteration
  }

  onSettings(): void {
    // Future: navigate to /settings — not in scope for this iteration
  }

  onLogout(): void {
    this.authService.signOut().subscribe(() => {
      void this.router.navigate(['/']);
    });
  }

  private loadGroups(): void {
    this.groupsLoading.set(true);
    this.groupsApi.getMyGroups().subscribe({
      next: groups => {
        this._groups.set(groups ?? []);
        this.groupsLoading.set(false);
      },
      error: () => {
        this._groups.set([]);
        this.groupsLoading.set(false);
      },
    });
  }
}
