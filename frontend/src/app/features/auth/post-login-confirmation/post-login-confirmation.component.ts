import { Component, input, output } from '@angular/core';

@Component({
  selector: 'tb-post-login-confirmation',
  standalone: true,
  templateUrl: './post-login-confirmation.component.html',
  styleUrl: './post-login-confirmation.component.scss',
})
export class PostLoginConfirmationComponent {
  readonly email = input.required<string>();
  readonly signOut = output<void>();

  onSignOut(): void {
    this.signOut.emit();
  }
}
