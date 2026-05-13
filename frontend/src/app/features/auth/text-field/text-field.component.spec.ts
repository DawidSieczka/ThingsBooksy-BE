import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { beforeEach, describe, expect, it } from 'vitest';
import { TextFieldComponent } from './text-field.component';

describe('TextFieldComponent', () => {
  let fixture: ComponentFixture<TextFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TextFieldComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TextFieldComponent);
    fixture.componentRef.setInput('label', 'Email');
    fixture.componentRef.setInput('icon', 'mail');
    fixture.componentRef.setInput('control', new FormControl<string>('', { nonNullable: true }));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
