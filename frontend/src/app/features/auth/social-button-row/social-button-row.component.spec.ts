import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { SocialButtonRowComponent } from './social-button-row.component';

describe('SocialButtonRowComponent', () => {
  let fixture: ComponentFixture<SocialButtonRowComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialButtonRowComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SocialButtonRowComponent);
    fixture.detectChanges();
  });

  it('should render the configured providers', () => {
    expect(fixture.componentInstance.providers).toHaveLength(2);
  });
});
