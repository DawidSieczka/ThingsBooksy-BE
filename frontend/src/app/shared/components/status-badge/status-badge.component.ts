import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-status-badge',
  standalone: true,
  imports: [],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadgeComponent {
  readonly status = input.required<'confirmed' | 'cancelled'>();
  readonly label = input<string>('');
}
