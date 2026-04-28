import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { toast } from '@spartan-ng/brain/sonner';
import { catchError, throwError } from 'rxjs';
import { AuthToken } from '@core/auth/auth-token';
import { CurrentUser } from '@core/auth/current-user';
import { Router } from '@angular/router';
import { ApiResponse } from './api-response';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authToken = inject(AuthToken);
  const currentUser = inject(CurrentUser);
  const router = inject(Router);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const isLoginRequest = req.url.endsWith('/auth/login');
      const isResetPasswordRequest = req.url.endsWith('/auth/change-password');

      const isAuthRequest = isLoginRequest || isResetPasswordRequest;

      if (err.status === HttpStatusCode.Unauthorized && !isAuthRequest) {
        authToken.clear();
        currentUser.clear();
        router.navigateByUrl('/login', { replaceUrl: true });
      } else if (err.status !== HttpStatusCode.Unauthorized || !isAuthRequest) {
        toast.error(resolveMessage(err));
      }

      return throwError(() => err);
    }),
  );
};

export function resolveMessage(err: HttpErrorResponse): string {
  if (err.status === 0) {
    return 'Unable to reach the server. Please check your connection.';
  }

  const body = err.error as ApiResponse<{ errors?: string[] }> | null;

  if (body?.data?.errors?.length) {
    return body.data.errors.join('\n');
  }

  if (body?.message) {
    return body.message;
  }

  return err.message;
}

/* function fallbackTitle(status: number): string {
  switch (status) {
    case HttpStatusCode.Unauthorized:
      return 'Unauthorized';
    case HttpStatusCode.Forbidden:
      return 'Forbidden';
    case HttpStatusCode.NotFound:
      return 'Not found';
    case HttpStatusCode.Conflict:
      return 'Conflict';
    case HttpStatusCode.InternalServerError:
      return 'Server error';
    default:
      return 'Something went wrong';
  }
} */
