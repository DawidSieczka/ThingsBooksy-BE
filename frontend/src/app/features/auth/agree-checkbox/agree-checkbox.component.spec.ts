import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { beforeEach, describe, expect, it } from 'vitest';
import { AgreeCheckboxComponent } from './agree-checkbox.component';

describe('AgreeCheckboxComponent', () => {
  let fixture: ComponentFixture<AgreeCheckboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AgreeCheckboxComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AgreeCheckboxComponent);
    fixture.componentRef.setInput('control', new FormControl<boolean>(false, { nonNullable: true }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
