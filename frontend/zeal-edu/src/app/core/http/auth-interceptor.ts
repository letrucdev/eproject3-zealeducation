import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthToken } from '../auth/auth-token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthToken).token();

  if (!token) {
    return next(req);
  }

  const authed = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(authed);
};
