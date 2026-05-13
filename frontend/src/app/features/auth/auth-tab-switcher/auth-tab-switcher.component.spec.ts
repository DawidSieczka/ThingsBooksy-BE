import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthTabSwitcherComponent } from './auth-tab-switcher.component';

describe('AuthTabSwitcherComponent', () => {
  let fixture: ComponentFixture<AuthTabSwitcherComponent>;
  let component: AuthTabSwitcherComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthTabSwitcherComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AuthTabSwitcherComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('mode', 'sign-in');
    fixture.detectChanges();
  });

  it('should emit modeChange when switching tabs', () => {
    const spy = vi.fn();
    component.modeChange.subscribe(spy);
    component.onSelect('sign-up');
    expect(spy).toHaveBeenCalledWith('sign-up');
  });

  it('should not emit when selecting the same mode', () => {
    const spy = vi.fn();
    component.modeChange.subscribe(spy);
    component.onSelect('sign-in');
    expect(spy).not.toHaveBeenCalled();
  });
});
