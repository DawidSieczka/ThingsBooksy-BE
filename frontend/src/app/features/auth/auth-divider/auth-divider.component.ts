import { Component, input } from '@angular/core';

@Component({
  selector: 'tb-auth-divider',
  standalone: true,
  templateUrl: './auth-divider.component.html',
  styleUrl: './auth-divider.component.scss',
})
export class AuthDividerComponent {
  readonly label = input<string>('or continue with');
}
