import { Routes } from '@angular/router';
import { authGuard } from '@core/auth/auth.guard';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    title: 'Login',
    loadComponent: () => import('./login/login'),
  },
  {
    path: 'change-password',
    title: 'Change password',
    canMatch: [authGuard],
    loadComponent: () => import('./change-password/change-password'),
  },
];
