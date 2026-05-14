import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { IconLogoutComponent } from './icon-logout.component';

describe('IconLogoutComponent', () => {
  let component: IconLogoutComponent;
  let fixture: ComponentFixture<IconLogoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconLogoutComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IconLogoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render an SVG element', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg).toBeTruthy();
  });

  it('should apply the default size of 15 to width and height', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('15');
    expect(svg.getAttribute('height')).toBe('15');
  });

  it('should apply a custom size when the size input is set', () => {
    fixture.componentRef.setInput('size', 24);
    fixture.detectChanges();

    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('24');
    expect(svg.getAttribute('height')).toBe('24');
  });

  it('should have aria-hidden set to true', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('aria-hidden')).toBe('true');
  });
});
