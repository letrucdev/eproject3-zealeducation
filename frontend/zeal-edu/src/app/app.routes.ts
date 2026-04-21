import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { hasRole } from './core/auth/role.guard';
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
    children: [
      {
        path: 'system',
        canMatch: [hasRole(UserRole.SystemAdmin)],
        loadChildren: () =>
          import('./features/system-admin/system-admin.routes').then((m) => m.SYSTEM_ADMIN_ROUTES),
      },
      { path: '', pathMatch: 'full', redirectTo: 'system/maintenance' },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'app' },
  { path: '**', redirectTo: 'app' },
];
