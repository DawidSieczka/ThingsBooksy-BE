import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'tb-animated-background',
  standalone: true,
  imports: [],
  templateUrl: './animated-background.component.html',
  styleUrl: './animated-background.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AnimatedBackgroundComponent {}
