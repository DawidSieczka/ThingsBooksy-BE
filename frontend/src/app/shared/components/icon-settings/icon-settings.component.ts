import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'tb-icon-settings',
  standalone: true,
  imports: [],
  templateUrl: './icon-settings.component.html',
  styleUrl: './icon-settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconSettingsComponent {
  readonly size = input<number>(15);
}
