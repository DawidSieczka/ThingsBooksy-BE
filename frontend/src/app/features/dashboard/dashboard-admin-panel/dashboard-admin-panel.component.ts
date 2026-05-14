import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GroupItem } from '../mock-data';
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
  readonly memberGroups = input.required<readonly GroupItem[]>();
  readonly adminGroups = input.required<readonly GroupItem[]>();

  readonly createGroupClicked = output<void>();
}
