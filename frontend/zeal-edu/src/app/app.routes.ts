import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { hasRole } from './core/auth/role.guard';
import { defaultRoleRedirect } from './core/layout/default-redirect';
import { UserRole } from './core/models/user-role';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'app',
    canMatch: [authGuard],
    loadComponent: () => import('./core/layout/app-shell').then((m) => m.AppShell),
    data: { breadcrumb: 'Home' },
    children: [
      {
        path: 'system',
        canMatch: [hasRole(UserRole.SystemAdmin)],
        data: { breadcrumb: 'System' },
        loadChildren: () =>
          import('./features/system-admin/system-admin.routes').then((m) => m.SYSTEM_ADMIN_ROUTES),
      },
      {
        path: 'counselor',
        canMatch: [hasRole(UserRole.Counselor)],
        data: { breadcrumb: 'Counselor' },
        loadChildren: () =>
          import('./features/counselor/counselor.routes').then((m) => m.COUNSELOR_ROUTES),
      },
      { path: '', pathMatch: 'full', canMatch: [defaultRoleRedirect], children: [] },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
