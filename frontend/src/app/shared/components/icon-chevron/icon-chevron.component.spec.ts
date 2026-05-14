import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { IconChevronComponent } from './icon-chevron.component';

describe('IconChevronComponent', () => {
  let component: IconChevronComponent;
  let fixture: ComponentFixture<IconChevronComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconChevronComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IconChevronComponent);
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

  it('should apply default size of 12 to width and height attributes', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('12');
    expect(svg.getAttribute('height')).toBe('12');
  });

  it('should apply custom size when size input is provided', () => {
    fixture.componentRef.setInput('size', 24);
    fixture.detectChanges();

    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('24');
    expect(svg.getAttribute('height')).toBe('24');
  });

  it('should not apply rotation transform when direction is "down" (default)', () => {
    const svg: HTMLElement = fixture.nativeElement.querySelector('svg');
    // direction is 'down' by default — transform should be null/empty
    expect(svg.style.transform).toBeFalsy();
  });

  it('should apply rotate(-90deg) transform when direction is "right"', () => {
    fixture.componentRef.setInput('direction', 'right');
    fixture.detectChanges();

    const svg: HTMLElement = fixture.nativeElement.querySelector('svg');
    expect(svg.style.transform).toBe('rotate(-90deg)');
  });

  it('should remove rotation when direction switches back to "down"', () => {
    fixture.componentRef.setInput('direction', 'right');
    fixture.detectChanges();

    fixture.componentRef.setInput('direction', 'down');
    fixture.detectChanges();

    const svg: HTMLElement = fixture.nativeElement.querySelector('svg');
    expect(svg.style.transform).toBeFalsy();
  });

  it('should have aria-hidden="true" on the SVG', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('aria-hidden')).toBe('true');
  });
});
