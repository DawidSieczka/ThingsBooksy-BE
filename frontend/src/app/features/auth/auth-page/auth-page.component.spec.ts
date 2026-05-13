import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { AuthPageComponent } from './auth-page.component';

describe('AuthPageComponent', () => {
  let fixture: ComponentFixture<AuthPageComponent>;
  let component: AuthPageComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should start unauthenticated', () => {
    expect(component.isAuthenticated()).toBe(false);
  });
});
