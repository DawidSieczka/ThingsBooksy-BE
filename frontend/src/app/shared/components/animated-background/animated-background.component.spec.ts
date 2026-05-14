import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { AnimatedBackgroundComponent } from './animated-background.component';

describe('AnimatedBackgroundComponent', () => {
  let component: AnimatedBackgroundComponent;
  let fixture: ComponentFixture<AnimatedBackgroundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnimatedBackgroundComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AnimatedBackgroundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the background container with aria-hidden="true"', () => {
    const el: HTMLElement = fixture.nativeElement;
    const bg = el.querySelector('.animated-bg');
    expect(bg).toBeTruthy();
    expect(bg?.getAttribute('aria-hidden')).toBe('true');
  });

  it('should render three orb elements', () => {
    const el: HTMLElement = fixture.nativeElement;
    const orbs = el.querySelectorAll('.animated-bg__orb');
    expect(orbs.length).toBe(3);
  });

  it('should render the grid overlay element', () => {
    const el: HTMLElement = fixture.nativeElement;
    const grid = el.querySelector('.animated-bg__grid');
    expect(grid).toBeTruthy();
  });

  it('should render five ring elements', () => {
    const el: HTMLElement = fixture.nativeElement;
    const rings = el.querySelectorAll('.animated-bg__ring');
    expect(rings.length).toBe(5);
  });
});
