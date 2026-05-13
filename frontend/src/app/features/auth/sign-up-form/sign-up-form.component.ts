import { Component, inject, input, output } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { AgreeCheckboxComponent } from '../agree-checkbox/agree-checkbox.component';
import { PasswordStrengthMeterComponent } from '../password-strength-meter/password-strength-meter.component';
import { TextFieldComponent } from '../text-field/text-field.component';

export interface SignUpFormValue {
  email: string;
  password: string;
}

function requiredAgreement(control: AbstractControl): ValidationErrors | null {
  return control.value === true ? null : { agreeRequired: true };
}

@Component({
  selector: 'tb-sign-up-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TextFieldComponent,
    PasswordStrengthMeterComponent,
    AgreeCheckboxComponent,
  ],
  templateUrl: './sign-up-form.component.html',
  styleUrl: './sign-up-form.component.scss',
})
export class SignUpFormComponent {
  private readonly fb = inject(FormBuilder);

  readonly isLoading = input<boolean>(false);
  readonly formSubmit = output<SignUpFormValue>();

  readonly form = this.fb.group({
    fullName: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2)],
    }),
    email: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
    agreed: this.fb.control(false, {
      nonNullable: true,
      validators: [requiredAgreement],
    }),
  });

  readonly passwordValue = toSignal(this.form.controls.password.valueChanges, {
    initialValue: this.form.controls.password.value,
  });

  readonly fullNameErrorMessages: Record<string, string> = {
    required: 'Please enter your name.',
    minlength: 'Name is too short.',
  };

  readonly emailErrorMessages: Record<string, string> = {
    required: 'Email is required.',
    email: 'Please enter a valid email address.',
  };

  readonly passwordErrorMessages: Record<string, string> = {
    required: 'Password is required.',
    minlength: 'Password must be at least 8 characters.',
  };

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    // fullName is collected for the UI but not sent to the backend
    // (backend SignUpRequest accepts only email + password; jobTitle/role default server-side).
    const value = this.form.getRawValue();
    this.formSubmit.emit({ email: value.email, password: value.password });
  }
}
