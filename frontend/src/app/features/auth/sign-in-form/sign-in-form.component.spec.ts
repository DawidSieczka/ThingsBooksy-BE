import { ComponentFixture, TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { SignInFormComponent } from './sign-in-form.component';

describe('SignInFormComponent', () => {
  let fixture: ComponentFixture<SignInFormComponent>;
  let component: SignInFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SignInFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(SignInFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should not emit submit when the form is invalid', () => {
    const spy = vi.fn();
    component.formSubmit.subscribe(spy);
    component.onSubmit();
    expect(spy).not.toHaveBeenCalled();
  });

  it('should emit submit with form value when valid', () => {
    const spy = vi.fn();
    component.formSubmit.subscribe(spy);
    component.form.setValue({ email: 'user@example.com', password: 'secret123' });
    component.onSubmit();
    expect(spy).toHaveBeenCalledWith({ email: 'user@example.com', password: 'secret123' });
  });
});
