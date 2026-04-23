import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { UserRole } from '@core/models/user-role';
import { CurrentUser } from './current-user';

export function hasRole(...roles: readonly UserRole[]): CanMatchFn {
  return () => {
    const role = inject(CurrentUser).role();

    if (role !== undefined && roles.includes(role)) return true;
    return inject(Router).parseUrl('/login');
  };
}
