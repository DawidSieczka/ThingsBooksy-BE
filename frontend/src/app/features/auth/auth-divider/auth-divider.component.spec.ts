import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { AuthDividerComponent } from './auth-divider.component';

describe('AuthDividerComponent', () => {
  let fixture: ComponentFixture<AuthDividerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthDividerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthDividerComponent);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
