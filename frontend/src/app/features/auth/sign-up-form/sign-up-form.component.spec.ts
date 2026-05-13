import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SignUpFormComponent } from './sign-up-form.component';

describe('SignUpFormComponent', () => {
  let fixture: ComponentFixture<SignUpFormComponent>;
  let component: SignUpFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SignUpFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SignUpFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should require the agree checkbox before submission', () => {
    const spy = vi.fn();
    component.formSubmit.subscribe(spy);
    component.form.patchValue({
      fullName: 'Jane Doe',
      email: 'jane@example.com',
      password: 'secret123',
    });
    component.onSubmit();
    expect(spy).not.toHaveBeenCalled();
  });

  it('should emit only email and password when fully valid', () => {
    const spy = vi.fn();
    component.formSubmit.subscribe(spy);
    component.form.setValue({
      fullName: 'Jane Doe',
      email: 'jane@example.com',
      password: 'secret123',
      agreed: true,
    });
    component.onSubmit();
    expect(spy).toHaveBeenCalledWith({ email: 'jane@example.com', password: 'secret123' });
  });
});
