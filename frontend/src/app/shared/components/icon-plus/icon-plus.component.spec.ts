import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { IconPlusComponent } from './icon-plus.component';

describe('IconPlusComponent', () => {
  let component: IconPlusComponent;
  let fixture: ComponentFixture<IconPlusComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconPlusComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IconPlusComponent);
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

  it('should use the default size of 14 for width and height attributes', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('14');
    expect(svg.getAttribute('height')).toBe('14');
  });

  it('should reflect a changed size input on the SVG width and height attributes', async () => {
    fixture.componentRef.setInput('size', 24);
    fixture.detectChanges();

    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('width')).toBe('24');
    expect(svg.getAttribute('height')).toBe('24');
  });

  it('should have aria-hidden="true" on the SVG', () => {
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');
    expect(svg.getAttribute('aria-hidden')).toBe('true');
  });
});
