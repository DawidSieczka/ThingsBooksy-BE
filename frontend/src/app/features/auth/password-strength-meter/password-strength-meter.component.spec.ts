import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { PasswordStrengthMeterComponent } from './password-strength-meter.component';

describe('PasswordStrengthMeterComponent', () => {
  let fixture: ComponentFixture<PasswordStrengthMeterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PasswordStrengthMeterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PasswordStrengthMeterComponent);
    fixture.componentRef.setInput('password', 'Abcd1234!Strong');
    fixture.detectChanges();
  });

  it('should classify a strong password as strong', () => {
    expect(fixture.componentInstance.result().tone).toBe('strong');
  });
});
