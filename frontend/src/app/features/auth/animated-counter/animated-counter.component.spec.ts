import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { AnimatedCounterComponent } from './animated-counter.component';

describe('AnimatedCounterComponent', () => {
  let fixture: ComponentFixture<AnimatedCounterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnimatedCounterComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AnimatedCounterComponent);
    fixture.componentRef.setInput('target', 100);
    fixture.componentRef.setInput('suffix', '+');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
