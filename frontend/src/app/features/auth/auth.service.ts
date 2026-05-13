import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { AuthError, CurrentUser, JsonWebTokenDto, SignInPayload, SignUpPayload } from './models/auth.model';
import { mapHttpErrorToAuthError, mapToCurrentUser } from './models/auth.mapper';

const TOKEN_STORAGE_KEY = 'tb.auth.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _token = signal<string | null>(this.readPersistedToken());
  private readonly _currentUser = signal<CurrentUser | null>(null);

  readonly token = this._token.asReadonly();
  readonly currentUser = this._currentUser.asReadonly();

  signIn(payload: SignInPayload): Observable<CurrentUser> {
    return this.http.post<JsonWebTokenDto>('/users/sign-in', payload).pipe(
      tap(jwt => this.persistToken(jwt.accessToken)),
      map(mapToCurrentUser),
      tap(user => this._currentUser.set(user)),
      catchError((error: HttpErrorResponse) => throwError(() => this.translate(error))),
    );
  }

  signUp(payload: SignUpPayload): Observable<void> {
    return this.http.post<void>('/users/sign-up', payload).pipe(
      catchError((error: HttpErrorResponse) => throwError(() => this.translate(error))),
    );
  }

  loadCurrentUser(): Observable<CurrentUser | null> {
    if (this._token() === null) {
      return of(null);
    }
    return this.http.get<JsonWebTokenDto>('/users/me').pipe(
      map(mapToCurrentUser),
      tap(user => this._currentUser.set(user)),
      catchError(() => {
        this.signOut();
        return of(null);
      }),
    );
  }

  signOut(): void {
    this._token.set(null);
    this._currentUser.set(null);
    try {
      localStorage.removeItem(TOKEN_STORAGE_KEY);
    } catch {
      // localStorage may be unavailable (SSR, private mode) — ignored
    }
  }

  private persistToken(token: string): void {
    this._token.set(token);
    try {
      localStorage.setItem(TOKEN_STORAGE_KEY, token);
    } catch {
      // localStorage may be unavailable — keep the in-memory signal only
    }
  }

  private readPersistedToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_STORAGE_KEY);
    } catch {
      return null;
    }
  }

  private translate(error: HttpErrorResponse): AuthError {
    return mapHttpErrorToAuthError(error);
  }
}
