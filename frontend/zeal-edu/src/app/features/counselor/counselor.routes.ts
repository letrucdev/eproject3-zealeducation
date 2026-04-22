import { Routes } from '@angular/router';

export const COUNSELOR_ROUTES: Routes = [
  {
    path: 'course-enquiries',
    title: 'Course Enquiries',
    data: { breadcrumb: 'Course Enquiries' },
    loadComponent: () => import('./course-enquiries/course-enquiries-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'course-enquiries' },
];
