import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-icon-chevron',
  standalone: true,
  imports: [],
  templateUrl: './icon-chevron.component.html',
  styleUrl: './icon-chevron.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconChevronComponent {
  readonly size = input<number>(12);
  readonly direction = input<'down' | 'right'>('down');
}
