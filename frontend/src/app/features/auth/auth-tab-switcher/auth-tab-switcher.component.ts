import { Component, input, output } from '@angular/core';

export type AuthMode = 'sign-in' | 'sign-up';

@Component({
  selector: 'tb-auth-tab-switcher',
  standalone: true,
  templateUrl: './auth-tab-switcher.component.html',
  styleUrl: './auth-tab-switcher.component.scss',
})
export class AuthTabSwitcherComponent {
  readonly mode = input.required<AuthMode>();
  readonly modeChange = output<AuthMode>();

  onSelect(mode: AuthMode): void {
    if (mode !== this.mode()) {
      this.modeChange.emit(mode);
    }
  }

  onKey(event: KeyboardEvent, mode: AuthMode): void {
    if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
      event.preventDefault();
      this.modeChange.emit(mode === 'sign-in' ? 'sign-up' : 'sign-in');
    }
  }
}
