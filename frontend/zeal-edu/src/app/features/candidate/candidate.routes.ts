import { Routes } from '@angular/router';

export const CANDIDATE_ROUTES: Routes = [
  {
    path: 'batches',
    title: 'My Batches',
    data: { breadcrumb: 'Batches' },
    loadComponent: () => import('./batches/my-batches-page'),
  },
  {
    path: 'batches/:id',
    title: 'Batch Detail',
    data: { breadcrumb: 'Detail' },
    loadComponent: () => import('./batches/batch-detail-page'),
  },
  {
    path: 'profile',
    title: 'Profile',
    data: { breadcrumb: 'Profile' },
    loadComponent: () => import('./profile/profile-page'),
  },
  {
    path: 'certificates',
    title: 'My Certificates',
    data: { breadcrumb: 'Certificates' },
    loadComponent: () => import('./certificates/my-certificates-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'batches' },
];
