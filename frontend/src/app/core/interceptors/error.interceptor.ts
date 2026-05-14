import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { EMPTY, catchError, throwError } from 'rxjs';
import { AuthService } from '../../features/auth/auth.service';
import { NotificationService } from '../../shared/services/notification.service';

const SILENT_HEADER = 'x-silent-errors';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const notifications = inject(NotificationService);
  const silent = req.headers.get(SILENT_HEADER) === 'true';

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        auth.signOut();
        return EMPTY;
      }
      if (!silent && error.status >= 400) {
        notifications.error(resolveMessage(error));
      }
      return throwError(() => error);
    }),
  );
};

function resolveMessage(error: HttpErrorResponse): string {
  const body = error.error as { message?: string; title?: string } | null;
  return body?.message ?? body?.title ?? error.statusText ?? 'Something went wrong.';
}
