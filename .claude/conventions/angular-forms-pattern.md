# angular-forms-pattern.md

## Rule

All forms in ThingsBooksy use Angular Reactive Forms with fully typed `FormControl<T>`. Template-driven forms (`ngModel`) are forbidden. Forms are built with `FormBuilder`, submitted via `firstValueFrom()` in the component, and never bound directly to HTTP service calls in the template.

---

## FormGroup skeleton

Use `FormBuilder` injected via `inject()`. All controls are explicitly typed.

```typescript
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ResourcesService } from '../resources.service';

@Component({
  selector: 'tb-create-resource-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './create-resource-form.component.html',
  styleUrl: './create-resource-form.component.scss',
})
export class CreateResourceFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly resourcesService = inject(ResourcesService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.group({
    name: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(3)],
    }),
    description: this.fb.control('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)],
    }),
  });

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isLoading.set(true);
    this.error.set(null);

    try {
      await firstValueFrom(this.resourcesService.createResource(this.form.getRawValue()));
      this.router.navigate(['/resources']);
    } catch {
      this.error.set('Nie udało się zapisać zasobu.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
```

Rules:
- Always use `nonNullable: true` on text controls so the value type is `string`, not `string | null`.
- Use `this.form.getRawValue()` on submit — it includes values from disabled controls.
- Check `this.form.invalid` at the top of `onSubmit()` and return early.
- `isLoading` and `error` are `signal()` — never raw class fields.

---

## Template binding

```html
<form [formGroup]="form" (ngSubmit)="onSubmit()">
  <div class="field">
    <label for="name">Nazwa</label>
    <input id="name" type="text" formControlName="name" />
    @if (form.controls.name.invalid && form.controls.name.touched) {
      <span class="error">
        @if (form.controls.name.errors?.['required']) { Pole wymagane. }
        @if (form.controls.name.errors?.['minlength']) { Minimum 3 znaki. }
      </span>
    }
  </div>

  @if (error()) {
    <p class="form-error">{{ error() }}</p>
  }

  <button type="submit" [disabled]="form.invalid || isLoading()">
    @if (isLoading()) { Zapisywanie… } @else { Zapisz }
  </button>
</form>
```

Rules:
- Always import `ReactiveFormsModule` in the component's `imports` array.
- Show validation errors only when the control is `touched` (user has interacted) or `dirty`.
- Use `form.controls.{name}.errors?.['validatorKey']` — never `form.get('name')`.
- Disable the submit button while `isLoading()` is `true` or while `form.invalid`.

---

## Sync validators

Place custom sync validators as standalone functions, not class methods.

```typescript
// shared/validators/no-whitespace.validator.ts
import { AbstractControl, ValidationErrors } from '@angular/forms';

export function noWhitespaceValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';
  return value.trim().length === 0 && value.length > 0
    ? { whitespace: true }
    : null;
}
```

Usage:

```typescript
name: this.fb.control('', {
  nonNullable: true,
  validators: [Validators.required, noWhitespaceValidator],
}),
```

Rules:
- Validator functions live in `shared/validators/` and are named `{rule}.validator.ts`.
- A validator returns `ValidationErrors | null` — never `boolean`.
- Do not throw inside a validator — return `null` for valid, an object for invalid.

---

## Async validators

Async validators use `timer(300)` to debounce before making an HTTP call. Using `control.valueChanges.pipe(debounceTime(...))` inside an `AsyncValidatorFn` is **forbidden** — it creates unclosed subscriptions and can cause infinite loops because Angular calls the validator function on every value change already.

```typescript
// shared/validators/unique-email.validator.ts
import { inject } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { catchError, first, map, switchMap } from 'rxjs/operators';
import { UsersService } from '../../features/users/users.service';

export function uniqueEmailValidator(): AsyncValidatorFn {
  const usersService = inject(UsersService);

  return (control: AbstractControl): Observable<ValidationErrors | null> =>
    timer(300).pipe(
      switchMap(() => usersService.checkEmailAvailability(control.value)),
      map(available => (available ? null : { emailTaken: true })),
      catchError(() => of(null)),
      first(),
    );
}
```

Usage:

```typescript
email: this.fb.control('', {
  nonNullable: true,
  validators: [Validators.required, Validators.email],
  asyncValidators: [uniqueEmailValidator()],
  updateOn: 'blur',
}),
```

Rules:
- Always use `timer(300)` (or a configurable delay) as the debounce mechanism — never `debounceTime` on `control.valueChanges`.
- Always pipe `first()` at the end to complete the Observable after one emission.
- Always `catchError(() => of(null))` — a failing HTTP call must not break the form.
- `inject()` is called outside the returned function, in the validator factory's scope.
- Use `updateOn: 'blur'` for async validators to avoid firing on every keystroke.
- Async validator functions live in `shared/validators/` alongside sync validators.

---

## `updateOn` strategy

| Scenario | `updateOn` |
|---|---|
| Sync-only validators, instant feedback desired | `'change'` (default) |
| Async validator (HTTP call) | `'blur'` |
| Search / filter fields | `'change'` |
| Password repeat field | `'blur'` |

Set `updateOn` at the control level:

```typescript
email: this.fb.control('', {
  nonNullable: true,
  validators: [Validators.required, Validators.email],
  asyncValidators: [uniqueEmailValidator()],
  updateOn: 'blur',
}),
```

---

## Cross-field validation

Use a group-level validator for rules that span multiple controls.

```typescript
export function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return password === confirm ? null : { passwordMismatch: true };
}
```

```typescript
readonly form = this.fb.group(
  {
    password: this.fb.control('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),
    confirmPassword: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
  },
  { validators: passwordMatchValidator },
);
```

Template — show cross-field error on the group, not on a specific control:

```html
@if (form.errors?.['passwordMismatch'] && form.touched) {
  <p class="error">Hasła nie pasują do siebie.</p>
}
```

---

## Resetting a form

Use `form.reset()` to restore all controls to their initial values and clear validation state.

```typescript
onCancel(): void {
  this.form.reset();
  this.error.set(null);
}
```

If the form must be pre-populated after loading data, use `form.patchValue()` (partial update) or `form.setValue()` (all controls required):

```typescript
ngOnInit(): void {
  this.resourceService.getResource(this.id).subscribe(resource => {
    this.form.patchValue({
      name: resource.name,
      description: resource.description,
    });
  });
}
```

Rules:
- Prefer `patchValue()` over `setValue()` — it is resilient to added/removed controls.
- Call `form.reset()` before navigation if the component is reused (e.g. router outlet without destroy).

---

## Zakazy — zestawienie

| Zakaz | Powód |
|---|---|
| Template-driven forms (`ngModel`) | Brak typowania, trudniejsze testowanie |
| `form.get('name')` | Używaj `form.controls.name` — silnie typowane |
| `nonNullable: false` (lub brak) na kontrolkach tekstowych | Wartość byłaby `string \| null` |
| `control.valueChanges.pipe(debounceTime(...))` w `AsyncValidatorFn` | Tworzy niezamknięte subskrypcje i może powodować nieskończone pętle; użyj `timer(300)` |
| Brakujący `first()` w async validatorze | Observable musi kompletować się po pierwszej emisji |
| Brakujący `catchError` w async validatorze | Błąd HTTP zepsuje formularz |
| `inject()` wewnątrz zwracanej funkcji walidatora | `inject()` musi być wywołany w kontekście DI — w fabryce, nie w callbacku |
| Bezpośrednie wywołanie serwisu HTTP w `(ngSubmit)` | Submit zawsze przez `onSubmit()` z `firstValueFrom()` |
