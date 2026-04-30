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
  {
    path: 'batches/:id',
    title: 'Batch Detail',
    data: { breadcrumb: 'Detail' },
    loadComponent: () => import('./batches/batch-detail-page'),
  },
  {
    path: 'batches/:batchId/sessions/:sessionId',
    title: 'Attendance',
    data: { breadcrumb: 'Attendance' },
    loadComponent: () => import('./batches/session-attendance-page'),
  },
  {
    path: 'candidates',
    title: 'Candidates',
    data: { breadcrumb: 'Candidates' },
    loadComponent: () => import('./candidates/candidates-management-page'),
  },
  {
    path: 'candidates/:id',
    title: 'Candidate Detail',
    data: { breadcrumb: 'Detail' },
    loadComponent: () => import('./candidates/candidate-detail-page'),
  },
  {
    path: 'materials',
    title: 'Materials',
    data: { breadcrumb: 'Materials' },
    loadComponent: () => import('./materials/materials-management-page'),
  },
  {
    path: 'feedback',
    title: 'Feedback',
    data: { breadcrumb: 'Feedback' },
    loadComponent: () => import('./feedback/feedback-management-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'courses' },
];
