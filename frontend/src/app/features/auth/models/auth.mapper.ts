import { HttpErrorResponse } from '@angular/common/http';
import { AuthError, AuthErrorResponse, CurrentUser, JsonWebTokenDto } from './auth.model';

export function mapToCurrentUser(jwt: JsonWebTokenDto): CurrentUser {
  return {
    id: jwt.userId,
    email: jwt.email,
    role: jwt.role,
  };
}

export function mapHttpErrorToAuthError(error: HttpErrorResponse): AuthError {
  const body = error.error as AuthErrorResponse | string | null;
  const firstCode = typeof body === 'object' && body !== null && Array.isArray(body.errors) && body.errors.length > 0
    ? body.errors[0].code
    : undefined;
  const firstMessage = typeof body === 'object' && body !== null && Array.isArray(body.errors) && body.errors.length > 0
    ? body.errors[0].message
    : undefined;

  if (firstMessage?.toLowerCase().includes('invalid credentials')) {
    return { kind: 'invalid-credentials', message: 'Invalid email or password.' };
  }
  if (firstMessage?.toLowerCase().includes('email is already in use')) {
    return { kind: 'email-in-use', message: 'This email is already registered.' };
  }
  if (error.status === 400 || error.status === 422) {
    return { kind: 'validation', message: firstMessage ?? 'Please check the form and try again.' };
  }
  return {
    kind: 'unknown',
    message: firstMessage ?? 'Something went wrong. Please try again.',
  };
}
