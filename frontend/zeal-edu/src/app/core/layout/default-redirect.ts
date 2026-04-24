import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { UserRole } from '@core/models/user-role';
import { CurrentUser } from '@core/auth/current-user';

export const defaultRoleRedirect: CanMatchFn = () => {
  const role = inject(CurrentUser).role();
  const router = inject(Router);

  switch (role) {
    case UserRole.SystemAdmin:
      return router.parseUrl('/app/system/maintenance');
    case UserRole.Counselor:
      return router.parseUrl('/app/counselor/course-enquiries');
    case UserRole.Incharge:
      return router.parseUrl('/app/incharge/courses');
    default:
      return router.parseUrl('/login');
  }
};
