import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { CountChipComponent } from './count-chip.component';

describe('CountChipComponent', () => {
  let component: CountChipComponent;
  let fixture: ComponentFixture<CountChipComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CountChipComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CountChipComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('count', 0);
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should render the count value', () => {
    fixture.componentRef.setInput('count', 42);
    fixture.detectChanges();
    const span: HTMLElement = fixture.nativeElement.querySelector('.chip');
    expect(span.textContent?.trim()).toBe('42');
  });

  it('should render count of zero', () => {
    fixture.componentRef.setInput('count', 0);
    fixture.detectChanges();
    const span: HTMLElement = fixture.nativeElement.querySelector('.chip');
    expect(span.textContent?.trim()).toBe('0');
  });

  it('should not have secondary class when accent is primary (default)', () => {
    fixture.componentRef.setInput('count', 5);
    fixture.detectChanges();
    const span: HTMLElement = fixture.nativeElement.querySelector('.chip');
    expect(span.classList.contains('secondary')).toBe(false);
  });

  it('should have secondary class when accent is secondary', () => {
    fixture.componentRef.setInput('count', 5);
    fixture.componentRef.setInput('accent', 'secondary');
    fixture.detectChanges();
    const span: HTMLElement = fixture.nativeElement.querySelector('.chip');
    expect(span.classList.contains('secondary')).toBe(true);
  });

  it('should not have secondary class when accent is explicitly primary', () => {
    fixture.componentRef.setInput('count', 10);
    fixture.componentRef.setInput('accent', 'primary');
    fixture.detectChanges();
    const span: HTMLElement = fixture.nativeElement.querySelector('.chip');
    expect(span.classList.contains('secondary')).toBe(false);
  });

  it('should render large count values', () => {
    fixture.componentRef.setInput('count', 9999);
    fixture.detectChanges();
    const span: HTMLElement = fixture.nativeElement.querySelector('.chip');
    expect(span.textContent?.trim()).toBe('9999');
  });
});
