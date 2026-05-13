import { Component, inject, output, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../auth.service';
import { AuthError, CurrentUser } from '../models/auth.model';
import { AuthDividerComponent } from '../auth-divider/auth-divider.component';
import { AuthMode, AuthTabSwitcherComponent } from '../auth-tab-switcher/auth-tab-switcher.component';
import { SignInFormComponent, SignInFormValue } from '../sign-in-form/sign-in-form.component';
import { SignUpFormComponent, SignUpFormValue } from '../sign-up-form/sign-up-form.component';
import { SocialButtonRowComponent } from '../social-button-row/social-button-row.component';
import { StatusMessageComponent } from '../status-message/status-message.component';

@Component({
  selector: 'tb-auth-card',
  standalone: true,
  imports: [
    AuthDividerComponent,
    AuthTabSwitcherComponent,
    SignInFormComponent,
    SignUpFormComponent,
    SocialButtonRowComponent,
    StatusMessageComponent,
  ],
  templateUrl: './auth-card.component.html',
  styleUrl: './auth-card.component.scss',
})
export class AuthCardComponent {
  private readonly authService = inject(AuthService);

  readonly mode = signal<AuthMode>('sign-in');
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly signedIn = output<CurrentUser>();

  onModeChange(mode: AuthMode): void {
    this.mode.set(mode);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  async onSignIn(value: SignInFormValue): Promise<void> {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isLoading.set(true);
    try {
      const user = await firstValueFrom(this.authService.signIn(value));
      this.successMessage.set('Signed in successfully.');
      this.signedIn.emit(user);
    } catch (err) {
      this.errorMessage.set(this.toMessage(err));
    } finally {
      this.isLoading.set(false);
    }
  }

  async onSignUp(value: SignUpFormValue): Promise<void> {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isLoading.set(true);
    try {
      await firstValueFrom(this.authService.signUp(value));
      // Auto sign-in for a smoother first-run experience.
      const user = await firstValueFrom(this.authService.signIn(value));
      this.successMessage.set('Account created. Welcome!');
      this.signedIn.emit(user);
    } catch (err) {
      this.errorMessage.set(this.toMessage(err));
    } finally {
      this.isLoading.set(false);
    }
  }

  private toMessage(err: unknown): string {
    if (typeof err === 'object' && err !== null && 'message' in err) {
      return (err as AuthError).message;
    }
    return 'Something went wrong. Please try again.';
  }
}
