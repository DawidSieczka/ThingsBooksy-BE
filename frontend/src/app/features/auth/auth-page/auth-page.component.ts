import { Component, OnInit, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AnimatedBackgroundComponent } from '../../../shared/components/animated-background/animated-background.component';
import { AnimatedCounterComponent } from '../animated-counter/animated-counter.component';
import { AuthCardComponent } from '../auth-card/auth-card.component';
import { AuthService } from '../auth.service';
import { CurrentUser } from '../models/auth.model';

interface Chip {
  label: string;
  color: string;
}

interface TickerItem {
  label: string;
  d: string;
}

@Component({
  selector: 'tb-auth-page',
  standalone: true,
  imports: [AnimatedBackgroundComponent, AnimatedCounterComponent, AuthCardComponent],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss',
})
export class AuthPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = this.authService.currentUser;
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  readonly chips: Chip[] = [
    { label: 'IT Equipment',        color: 'var(--color-accent-primary)' },
    { label: 'Meeting Rooms',       color: 'var(--color-accent-secondary)' },
    { label: 'Company Vehicles',    color: 'var(--color-accent-tertiary)' },
    { label: 'Barber Appointments', color: 'var(--color-accent-primary)' },
    { label: 'Wellness Sessions',   color: 'var(--color-accent-secondary)' },
  ];

  readonly tickerItems: TickerItem[] = [
    { label: 'Laptop',       d: 'M4 16h16M4 16V6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v10M2 20h20' },
    { label: 'Meeting Room', d: 'M8 2v4M16 2v4M3 10h18M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z' },
    { label: 'Company Car',  d: 'M5 17H3a2 2 0 0 1-2-2V9l3-6h16l3 6v6a2 2 0 0 1-2 2h-2m-9 0h4M7.5 17a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5zm9 0a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z' },
    { label: 'Barber',       d: 'M6 3a3 3 0 1 1 0 6 3 3 0 0 1 0-6zm0 12a3 3 0 1 1 0 6 3 3 0 0 1 0-6zM20 4 8.12 15.88M14.47 14.48 20 20M8.12 8.12 12 12' },
    { label: 'Wellness',     d: 'M6 4v6a6 6 0 0 0 12 0V4M4 4h4m8 0h4M12 22v-4' },
    { label: 'Office Keys',  d: 'M2.586 17.414A2 2 0 0 0 2 18.828V21a1 1 0 0 0 1 1h3a1 1 0 0 0 1-1v-1a1 1 0 0 0 1-1h1a1 1 0 0 0 1-1v-1a1 1 0 0 0 1-1h.172a2 2 0 0 0 1.414-.586l.814-.814a6.5 6.5 0 1 0-4-4z' },
    { label: 'Monitor',      d: 'M2 3h20v14H2zM8 21h8m-4-4v4' },
    { label: 'Projector',    d: 'M5 7H3a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h18a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-2M5 7V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v2M5 7h14M12 12a3 3 0 1 0 0 6 3 3 0 0 0 0-6z' },
    { label: 'Camera',       d: 'M14.5 4h-5L7 7H4a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-3zM12 17a4 4 0 1 0 0-8 4 4 0 0 0 0 8z' },
    { label: 'Parking Spot', d: 'M5 22V2h7a5 5 0 0 1 0 10H5m5 0v10' },
    { label: 'Yoga Studio',  d: 'M12 2a2 2 0 1 0 0 4 2 2 0 0 0 0-4zM7 8l-3 6 4-1 1 5 3-4 3 4 1-5 4 1-3-6' },
    { label: 'Toolkit',      d: 'M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z' },
  ];

  // Doubled list so the ticker can scroll seamlessly with translateX(-50% → 0)
  readonly tickerLoop: TickerItem[] = [...this.tickerItems, ...this.tickerItems];

  async ngOnInit(): Promise<void> {
    if (this.authService.token() !== null && this.currentUser() === null) {
      try {
        await firstValueFrom(this.authService.loadCurrentUser());
      } catch {
        // loadCurrentUser clears state on failure.
      }
    }
    if (this.isAuthenticated()) {
      void this.router.navigate(['/dashboard']);
    }
  }

  onSignedIn(_user: CurrentUser): void {
    void this.router.navigate(['/dashboard']);
  }
}
