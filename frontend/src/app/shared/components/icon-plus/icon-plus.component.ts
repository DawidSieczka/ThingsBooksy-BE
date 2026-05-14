import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-icon-plus',
  standalone: true,
  imports: [],
  templateUrl: './icon-plus.component.html',
  styleUrl: './icon-plus.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconPlusComponent {
  readonly size = input<number>(14);
}
