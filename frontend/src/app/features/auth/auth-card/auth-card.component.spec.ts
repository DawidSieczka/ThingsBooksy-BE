import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { AuthCardComponent } from './auth-card.component';

describe('AuthCardComponent', () => {
  let fixture: ComponentFixture<AuthCardComponent>;
  let component: AuthCardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthCardComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should default to sign-in mode', () => {
    expect(component.mode()).toBe('sign-in');
  });

  it('should clear messages when switching tabs', () => {
    component.errorMessage.set('boom');
    component.onModeChange('sign-up');
    expect(component.errorMessage()).toBeNull();
    expect(component.mode()).toBe('sign-up');
  });
});
