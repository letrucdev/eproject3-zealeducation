import { Routes } from '@angular/router';

export const INCHARGE_ROUTES: Routes = [
  {
    path: 'courses',
    title: 'Courses',
    data: { breadcrumb: 'Courses' },
    loadComponent: () => import('./courses/courses-management-page'),
  },
  {
    path: 'batches',
    title: 'Batches',
    data: { breadcrumb: 'Batches' },
    loadComponent: () => import('./batches/batches-management-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'courses' },
];
