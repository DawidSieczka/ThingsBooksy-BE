import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-count-chip',
  standalone: true,
  imports: [],
  templateUrl: './count-chip.component.html',
  styleUrl: './count-chip.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CountChipComponent {
  readonly count = input.required<number>();
  readonly accent = input<'primary' | 'secondary'>('primary');
}
