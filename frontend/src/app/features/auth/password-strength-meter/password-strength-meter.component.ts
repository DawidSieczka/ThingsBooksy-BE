import { Component, computed, input } from '@angular/core';

interface StrengthResult {
  score: 0 | 1 | 2 | 3 | 4;
  label: string;
  tone: 'idle' | 'weak' | 'fair' | 'good' | 'strong';
}

function evaluate(password: string): StrengthResult {
  if (!password) {
    return { score: 0, label: '', tone: 'idle' };
  }
  let score = 0;
  if (password.length >= 8) score++;
  if (/[A-Z]/.test(password) && /[a-z]/.test(password)) score++;
  if (/\d/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password) && password.length >= 10) score++;

  switch (score) {
    case 0:
    case 1: return { score: 1, label: 'Too weak', tone: 'weak' };
    case 2: return { score: 2, label: 'Weak', tone: 'fair' };
    case 3: return { score: 3, label: 'Good', tone: 'good' };
    default: return { score: 4, label: 'Strong', tone: 'strong' };
  }
}

@Component({
  selector: 'tb-password-strength-meter',
  standalone: true,
  templateUrl: './password-strength-meter.component.html',
  styleUrl: './password-strength-meter.component.scss',
})
export class PasswordStrengthMeterComponent {
  readonly password = input.required<string>();

  readonly result = computed<StrengthResult>(() => evaluate(this.password()));
  readonly bars = [1, 2, 3, 4] as const;
}
