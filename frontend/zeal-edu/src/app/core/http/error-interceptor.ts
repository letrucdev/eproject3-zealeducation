import { HttpErrorResponse, HttpInterceptorFn, HttpStatusCode } from '@angular/common/http';
import { inject } from '@angular/core';
import { toast } from '@spartan-ng/brain/sonner';
import { catchError, throwError } from 'rxjs';
import { AuthToken } from '../auth/auth-token';
import { CurrentUser } from '../auth/current-user';
import { Router } from '@angular/router';
import { ApiResponse } from './api-response';

interface ApiErrorBody {
  message?: string;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authToken = inject(AuthToken);
  const currentUser = inject(CurrentUser);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      const isLoginRequest = req.url.endsWith('/auth/login');

      if (err.status === HttpStatusCode.Unauthorized && !isLoginRequest) {
        authToken.clear();
        currentUser.clear();
        inject(Router).navigateByUrl('/login');
      }

      /*      if (err.status === HttpStatusCode.BadRequest) {
        toast.error(resolveValidateError(err));
      } */

      if (err.status !== HttpStatusCode.Unauthorized || !isLoginRequest) {
        toast.error(resolveMessage(err));
      }

      return throwError(() => err);
    }),
  );
};

function resolveValidateError(err: HttpErrorResponse): string {
  const body = err.error as ApiResponse<{ errors: [] }>;

  return body.data?.errors.join(', ') ?? body.message;
}

function resolveMessage(err: HttpErrorResponse): string {
  if (err.status === 0) {
    return 'Unable to reach the server. Please check your connection.';
  }

  /* const body = err.error as ApiErrorBody | string | null;

  if (typeof body === 'string' && body.trim().length > 0) {
    return body;
  }

  const message = (body as ApiErrorBody | null)?.message?.trim();
  if (message) {
    return message;
  } */

  const body = err.error as ApiResponse<{ errors: [] }>;
  if (body) {
    return body.data?.errors.join(', ') ?? body.message;
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
