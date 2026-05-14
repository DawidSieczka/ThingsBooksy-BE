import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-icon-logout',
  standalone: true,
  imports: [],
  templateUrl: './icon-logout.component.html',
  styleUrl: './icon-logout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconLogoutComponent {
  readonly size = input<number>(15);
}
