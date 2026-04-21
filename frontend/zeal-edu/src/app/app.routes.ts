import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { hasRole } from './core/auth/role.guard';
import { UserRole } from './core/models/user-role';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login'),
  },
  {
    path: 'app',
    canMatch: [authGuard],
    loadComponent: () => import('./core/layout/app-shell').then((m) => m.AppShell),
    children: [
      {
        path: 'system',
        canMatch: [hasRole(UserRole.SystemAdmin)],
        children: [
          {
            path: 'maintenance',
            loadComponent: () => import('./features/system-admin/maintenance/maintenance-page'),
          },
          {
            path: 'staff-accounts',
            loadComponent: () =>
              import('./features/system-admin/staff-accounts/staff-accounts-page'),
          },
          { path: '', pathMatch: 'full', redirectTo: 'maintenance' },
        ],
      },
      { path: '', pathMatch: 'full', redirectTo: 'system/maintenance' },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'app' },
  { path: '**', redirectTo: 'app' },
];
