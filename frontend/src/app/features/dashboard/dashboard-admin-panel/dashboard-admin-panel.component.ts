import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GroupListItemDto } from '../../groups/services/groups-api.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { CountChipComponent } from '../../../shared/components/count-chip/count-chip.component';
import { IconPlusComponent } from '../../../shared/components/icon-plus/icon-plus.component';

@Component({
  selector: 'tb-dashboard-admin-panel',
  standalone: true,
  imports: [AvatarComponent, CountChipComponent, IconPlusComponent, RouterLink],
  templateUrl: './dashboard-admin-panel.component.html',
  styleUrl: './dashboard-admin-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardAdminPanelComponent {
  readonly memberGroups = input.required<readonly GroupListItemDto[]>();
  readonly adminGroups = input.required<readonly GroupListItemDto[]>();
  readonly loading = input<boolean>(false);

  readonly createGroupClicked = output<void>();

  initialsOf(name: string): string {
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }
}
