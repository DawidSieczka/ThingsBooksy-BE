import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-icon-close',
  standalone: true,
  imports: [],
  templateUrl: './icon-close.component.html',
  styleUrl: './icon-close.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconCloseComponent {
  readonly size = input<number>(16);
}
