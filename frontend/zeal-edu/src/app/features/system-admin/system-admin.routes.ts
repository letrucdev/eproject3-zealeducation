import { Routes } from '@angular/router';

export const SYSTEM_ADMIN_ROUTES: Routes = [
  {
    path: 'maintenance',
    title: 'System Maintenance',
    data: { breadcrumb: 'System Maintenance' },
    loadComponent: () => import('./maintenance/maintenance-page'),
  },
  {
    path: 'staff-accounts',
    title: 'Staff Accounts',
    data: { breadcrumb: 'Staff Accounts' },
    loadComponent: () => import('./staff-accounts/staff-accounts-page'),
  },
  {
    path: 'audit-log',
    title: 'Audit Log',
    data: { breadcrumb: 'Audit Log' },
    loadComponent: () => import('./audit-log/audit-log-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'maintenance' },
];
