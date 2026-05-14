import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';

@Component({
  selector: 'tb-dashboard-header',
  standalone: true,
  imports: [UserMenuComponent],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardHeaderComponent {
  readonly userName = input.required<string>();
  readonly userInitials = input.required<string>();

  readonly settingsClicked = output<void>();
  readonly logoutClicked = output<void>();
}
