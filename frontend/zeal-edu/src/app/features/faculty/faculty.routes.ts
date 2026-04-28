import { Routes } from '@angular/router';

export const FACULTY_ROUTES: Routes = [
  {
    path: 'schedule',
    title: 'Teaching Schedule',
    data: { breadcrumb: 'Schedule' },
    loadComponent: () => import('./schedule/schedule-page'),
  },
  {
    path: 'batches',
    title: 'My Batches',
    data: { breadcrumb: 'Batches' },
    loadComponent: () => import('./batches/faculty-batches-page'),
  },
  {
    path: 'batches/:id',
    title: 'Batch Detail',
    data: { breadcrumb: 'Detail' },
    loadComponent: () => import('./batches/faculty-batch-detail-page'),
  },
  {
    path: 'batches/:batchId/sessions/:sessionId',
    title: 'Attendance',
    data: { breadcrumb: 'Attendance' },
    loadComponent: () => import('./batches/faculty-session-attendance-page'),
  },
  {
    path: 'examinations',
    title: 'Examinations',
    data: { breadcrumb: 'Examinations' },
    loadComponent: () => import('./exams/faculty-examinations-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'schedule' },
];
