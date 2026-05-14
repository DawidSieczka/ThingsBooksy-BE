import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
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

  // Email-derived placeholder until BE adds FirstName/LastName (see specs/007).
  readonly displayName = computed(() => {
    const email = this._currentUser()?.email;
    if (!email) return '';
    const local = email.split('@')[0];
    return local
      .split(/[._-]+/)
      .filter(Boolean)
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  });

  readonly initials = computed(() => {
    const email = this._currentUser()?.email;
    if (!email) return '';
    const local = email.split('@')[0];
    const parts = local.split(/[._-]+/).filter(Boolean);
    if (parts.length === 0) return '';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  });

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
        // Bootstrap path: token is invalid. Clear local state without hitting /users/logout
        // (the server has nothing to revoke — auth-interceptor will route 401s here).
        this._token.set(null);
        this._currentUser.set(null);
        try {
          localStorage.removeItem(TOKEN_STORAGE_KEY);
        } catch {
          // ignored
        }
        return of(null);
      }),
    );
  }

  signOut(): Observable<void> {
    // Best-effort server-side revocation; we always clear local state on completion.
    return this.http.post<void>('/users/logout', null).pipe(
      catchError(() => of(undefined)),
      tap(() => {
        this._token.set(null);
        this._currentUser.set(null);
        try {
          localStorage.removeItem(TOKEN_STORAGE_KEY);
        } catch {
          // localStorage may be unavailable (SSR, private mode) — ignored
        }
      }),
      map(() => undefined),
    );
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
