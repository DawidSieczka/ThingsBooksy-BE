import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { StatusBadgeComponent } from './status-badge.component';
import { By } from '@angular/platform-browser';
import { Component } from '@angular/core';

@Component({
  standalone: true,
  imports: [StatusBadgeComponent],
  template: `<tb-status-badge [status]="status" [label]="label" />`,
})
class TestHostComponent {
  status: 'confirmed' | 'cancelled' = 'confirmed';
  label = '';
}

describe('StatusBadgeComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHostComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    const badge = fixture.debugElement.query(By.directive(StatusBadgeComponent));
    expect(badge).toBeTruthy();
  });

  it('should render "Confirmed" default label for confirmed status', () => {
    host.status = 'confirmed';
    fixture.detectChanges();
    const span: HTMLElement = fixture.debugElement.query(By.css('.badge')).nativeElement;
    expect(span.textContent?.trim()).toBe('Confirmed');
  });

  it('should render "Cancelled" default label for cancelled status', () => {
    host.status = 'cancelled';
    fixture.detectChanges();
    const span: HTMLElement = fixture.debugElement.query(By.css('.badge')).nativeElement;
    expect(span.textContent?.trim()).toBe('Cancelled');
  });

  it('should apply the confirmed CSS class for confirmed status', () => {
    host.status = 'confirmed';
    fixture.detectChanges();
    const span: HTMLElement = fixture.debugElement.query(By.css('.badge')).nativeElement;
    expect(span.classList).toContain('confirmed');
    expect(span.classList).not.toContain('cancelled');
  });

  it('should apply the cancelled CSS class for cancelled status', () => {
    host.status = 'cancelled';
    fixture.detectChanges();
    const span: HTMLElement = fixture.debugElement.query(By.css('.badge')).nativeElement;
    expect(span.classList).toContain('cancelled');
    expect(span.classList).not.toContain('confirmed');
  });

  it('should render the override label when label input is provided', () => {
    host.status = 'confirmed';
    host.label = 'Active';
    fixture.detectChanges();
    const span: HTMLElement = fixture.debugElement.query(By.css('.badge')).nativeElement;
    expect(span.textContent?.trim()).toBe('Active');
  });

  it('should render override label for cancelled status when label is provided', () => {
    host.status = 'cancelled';
    host.label = 'Rejected';
    fixture.detectChanges();
    const span: HTMLElement = fixture.debugElement.query(By.css('.badge')).nativeElement;
    expect(span.textContent?.trim()).toBe('Rejected');
  });
});
