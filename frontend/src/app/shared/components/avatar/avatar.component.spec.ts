import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { AvatarComponent } from './avatar.component';
import { ComponentRef } from '@angular/core';

function fnv1a32(input: string): number {
  let hash = 0x811c9dc5;
  for (let i = 0; i < input.length; i++) {
    hash ^= input.charCodeAt(i);
    hash = (hash * 0x01000193) >>> 0;
  }
  return hash >>> 0;
}

describe('AvatarComponent', () => {
  let component: AvatarComponent;
  let fixture: ComponentFixture<AvatarComponent>;
  let componentRef: ComponentRef<AvatarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AvatarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AvatarComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    componentRef.setInput('id', 'test-id-1');
    componentRef.setInput('initials', 'AB');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render initials text in the template', () => {
    const initialsEl = fixture.nativeElement.querySelector('.avatar__initials');
    expect(initialsEl).toBeTruthy();
    expect(initialsEl.textContent.trim()).toBe('AB');
  });

  it('should have correct aria-label derived from initials', () => {
    const avatarEl = fixture.nativeElement.querySelector('.avatar');
    expect(avatarEl.getAttribute('aria-label')).toBe('AB avatar');
  });

  it('should derive the same color for the same id on multiple renders', () => {
    const colorFirst = component.resolvedColor();

    componentRef.setInput('initials', 'XY');
    fixture.detectChanges();

    const colorSecond = component.resolvedColor();
    expect(colorFirst).toBe(colorSecond);
  });

  it('should derive color deterministically from id via FNV-1a hash', () => {
    const testId = 'deterministic-test-id';
    componentRef.setInput('id', testId);
    fixture.detectChanges();

    const bucket = fnv1a32(testId) % 3;
    const expectedColors = ['primary', 'secondary', 'tertiary'];
    const expectedColor = expectedColors[bucket];

    expect(component.resolvedColor()).toBe(expectedColor);
  });

  it('should produce potentially different colors for different ids', () => {
    componentRef.setInput('id', 'id-bucket-0-primary');
    fixture.detectChanges();
    const colorA = component.resolvedColor();

    componentRef.setInput('id', 'id-bucket-secondary');
    fixture.detectChanges();
    const colorB = component.resolvedColor();

    const bucketA = fnv1a32('id-bucket-0-primary') % 3;
    const bucketB = fnv1a32('id-bucket-secondary') % 3;

    const colors = ['primary', 'secondary', 'tertiary'];
    expect(component.resolvedColor()).toBe(colors[bucketB]);
    expect(colorA).toBe(colors[bucketA]);
    // Different hashes can legitimately map to same bucket, so we assert each deterministically
    expect(bucketA).not.toBe(bucketB);
    expect(colorA).not.toBe(colorB);
  });

  it('should use colorOverride when provided, bypassing hash', () => {
    componentRef.setInput('id', 'any-id');
    componentRef.setInput('colorOverride', 'tertiary');
    fixture.detectChanges();

    expect(component.resolvedColor()).toBe('tertiary');
  });

  it('should use colorOverride secondary, bypassing hash', () => {
    componentRef.setInput('id', 'any-id');
    componentRef.setInput('colorOverride', 'secondary');
    fixture.detectChanges();

    expect(component.resolvedColor()).toBe('secondary');
  });

  it('should revert to hash-derived color when colorOverride is undefined', () => {
    const testId = 'revert-test-id';
    componentRef.setInput('id', testId);
    componentRef.setInput('colorOverride', 'tertiary');
    fixture.detectChanges();
    expect(component.resolvedColor()).toBe('tertiary');

    componentRef.setInput('colorOverride', undefined);
    fixture.detectChanges();
    const bucket = fnv1a32(testId) % 3;
    const colors = ['primary', 'secondary', 'tertiary'];
    expect(component.resolvedColor()).toBe(colors[bucket]);
  });

  it('should apply md size styles by default', () => {
    const styles = component.hostStyles();
    expect(styles['--avatar-size']).toBe('2.75rem');
    expect(styles['--avatar-font-size']).toBe('var(--font-size-base)');
    expect(styles['--avatar-radius']).toBe('0.6875rem');
  });

  it('should apply sm size styles when size input is sm', () => {
    componentRef.setInput('size', 'sm');
    fixture.detectChanges();
    const styles = component.hostStyles();
    expect(styles['--avatar-size']).toBe('2.125rem');
    expect(styles['--avatar-font-size']).toBe('var(--font-size-sm)');
    expect(styles['--avatar-radius']).toBe('0.5625rem');
  });

  it('should apply lg size styles when size input is lg', () => {
    componentRef.setInput('size', 'lg');
    fixture.detectChanges();
    const styles = component.hostStyles();
    expect(styles['--avatar-size']).toBe('3.875rem');
    expect(styles['--avatar-font-size']).toBe('var(--font-size-2xl)');
    expect(styles['--avatar-radius']).toBe('0.9375rem');
  });
});
