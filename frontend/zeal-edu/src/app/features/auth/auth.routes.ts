import { Routes } from '@angular/router';

export const AUTH_ROUTES: Routes = [
  {
    path: 'login',
    title: 'Login',
    loadComponent: () => import('./login/login'),
  },
];
