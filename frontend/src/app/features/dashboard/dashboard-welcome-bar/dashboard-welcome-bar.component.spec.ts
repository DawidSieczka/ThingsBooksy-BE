import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { DashboardWelcomeBarComponent } from './dashboard-welcome-bar.component';

describe('DashboardWelcomeBarComponent', () => {
  let component: DashboardWelcomeBarComponent;
  let fixture: ComponentFixture<DashboardWelcomeBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardWelcomeBarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardWelcomeBarComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('userName', 'Alice');
    fixture.componentRef.setInput('date', new Date('2025-05-14'));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the greeting label', () => {
    const hi = fixture.nativeElement.querySelector('.welcome-bar__hi') as HTMLElement;
    expect(hi.textContent?.trim()).toBe('Hi,');
  });

  it('should render the user name', () => {
    const name = fixture.nativeElement.querySelector('.welcome-bar__name') as HTMLElement;
    expect(name.textContent?.trim()).toBe('Alice');
  });

  it('should render the formatted date', () => {
    const dateEl = fixture.nativeElement.querySelector('.welcome-bar__date') as HTMLElement;
    expect(dateEl.textContent?.trim()).toBe('May 14, 2025');
  });
});
