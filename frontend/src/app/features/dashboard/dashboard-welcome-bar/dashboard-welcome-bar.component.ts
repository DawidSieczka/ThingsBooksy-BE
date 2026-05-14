import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'tb-dashboard-welcome-bar',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './dashboard-welcome-bar.component.html',
  styleUrl: './dashboard-welcome-bar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardWelcomeBarComponent {
  readonly userName = input.required<string>();
  readonly date = input.required<Date>();
}
