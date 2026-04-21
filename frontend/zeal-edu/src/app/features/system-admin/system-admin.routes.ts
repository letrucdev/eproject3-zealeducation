import { Routes } from '@angular/router';

export const SYSTEM_ADMIN_ROUTES: Routes = [
  {
    path: 'maintenance',
    loadComponent: () => import('./maintenance/maintenance-page'),
  },
  {
    path: 'staff-accounts',
    loadComponent: () => import('./staff-accounts/staff-accounts-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'maintenance' },
];
