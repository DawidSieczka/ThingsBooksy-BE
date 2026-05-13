import { Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TextFieldComponent } from '../text-field/text-field.component';

export interface SignInFormValue {
  email: string;
  password: string;
}

@Component({
  selector: 'tb-sign-in-form',
  standalone: true,
  imports: [ReactiveFormsModule, TextFieldComponent],
  templateUrl: './sign-in-form.component.html',
  styleUrl: './sign-in-form.component.scss',
})
export class SignInFormComponent {
  private readonly fb = inject(FormBuilder);

  readonly isLoading = input<boolean>(false);
  readonly formSubmit = output<SignInFormValue>();
  readonly forgotPassword = output<void>();

  readonly form = this.fb.group({
    email: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
  });

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
    this.formSubmit.emit(this.form.getRawValue());
  }

  onForgotPassword(event: Event): void {
    event.preventDefault();
    this.forgotPassword.emit();
  }
}
