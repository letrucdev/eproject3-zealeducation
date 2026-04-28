import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { CurrentUser } from './current-user';

export const mustChangePasswordGuard: CanMatchFn = () => {
  const currentUser = inject(CurrentUser);
  if (!currentUser.mustChangePassword()) return true;
  return inject(Router).parseUrl('/change-password');
};
