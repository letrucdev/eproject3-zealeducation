import { HttpEventType, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs';
import { LoginResponse } from '@features/auth/models/login-response';
import { AuthToken } from '@core/auth/auth-token';
import { CurrentUser } from '@core/auth/current-user';
import { ApiResponse } from './api-response';

export const authPersistenceInterceptor: HttpInterceptorFn = (req, next) => {
  const authToken = inject(AuthToken);
  const currentUser = inject(CurrentUser);

  return next(req).pipe(
    tap((event) => {
      if (event.type !== HttpEventType.Response) return;
      if (!req.url.endsWith('/auth/login')) return;

      const body = event.body as ApiResponse<LoginResponse> | null;
      if (!body?.data) return;

      authToken.set(body.data.token, body.data.expiresAt);
      currentUser.set(body.data.user);
    }),
  );
};
