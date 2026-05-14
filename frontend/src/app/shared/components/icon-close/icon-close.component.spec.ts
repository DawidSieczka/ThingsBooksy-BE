import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { IconCloseComponent } from './icon-close.component';

describe('IconCloseComponent', () => {
  let component: IconCloseComponent;
  let fixture: ComponentFixture<IconCloseComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconCloseComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IconCloseComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render an SVG element', () => {
    const svg: HTMLElement = fixture.nativeElement.querySelector('svg');
    expect(svg).toBeTruthy();
  });

  it('should apply the default size of 16 to width and height', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('16');
    expect(svg.getAttribute('height')).toBe('16');
  });

  it('should apply a custom size when the size input is set', async () => {
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
