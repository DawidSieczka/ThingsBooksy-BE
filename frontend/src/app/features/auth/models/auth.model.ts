export interface SignInPayload {
  email: string;
  password: string;
}

export interface SignUpPayload {
  email: string;
  password: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  role: string;
}

export interface JsonWebTokenDto {
  accessToken: string;
  expiry: number;
  userId: string;
  email: string;
  role: string;
  claims?: Record<string, string[]>;
}

export interface AuthErrorResponse {
  errors: { code: string; message: string }[];
}

export type AuthErrorKind = 'invalid-credentials' | 'email-in-use' | 'validation' | 'unknown';

export interface AuthError {
  kind: AuthErrorKind;
  message: string;
}
