import { Component } from '@angular/core';

interface SocialProvider {
  id: 'google' | 'linkedin';
  label: string;
}

@Component({
  selector: 'tb-social-button-row',
  standalone: true,
  templateUrl: './social-button-row.component.html',
  styleUrl: './social-button-row.component.scss',
})
export class SocialButtonRowComponent {
  readonly providers: SocialProvider[] = [
    { id: 'google', label: 'Google' },
    { id: 'linkedin', label: 'LinkedIn' },
  ];

  onSelect(_provider: SocialProvider): void {
    // No-op: visual placeholder until OAuth providers are wired.
  }
}
