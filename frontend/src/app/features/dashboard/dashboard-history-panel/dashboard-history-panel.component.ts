import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { CountChipComponent } from '../../../shared/components/count-chip/count-chip.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { HistoryRow } from '../mock-data';

@Component({
  selector: 'tb-dashboard-history-panel',
  standalone: true,
  imports: [DatePipe, CountChipComponent, StatusBadgeComponent],
  templateUrl: './dashboard-history-panel.component.html',
  styleUrl: './dashboard-history-panel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardHistoryPanelComponent {
  readonly rows = input.required<readonly HistoryRow[]>();
  readonly viewAllClicked = output<void>();
}
