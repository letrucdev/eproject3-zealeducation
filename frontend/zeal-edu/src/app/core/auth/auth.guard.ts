import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthToken } from './auth-token';

export const authGuard: CanMatchFn = () => {
  const token = inject(AuthToken).token();
  if (token) return true;
  return inject(Router).parseUrl('/login');
};
