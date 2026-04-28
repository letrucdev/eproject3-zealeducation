import { Routes } from '@angular/router';

export const ACCOUNTS_ROUTES: Routes = [
  {
    path: 'payments',
    title: 'Payments',
    data: { breadcrumb: 'Payments' },
    loadComponent: () => import('./payments/payments-list-page'),
  },
  {
    path: 'payments/:feeId',
    title: 'Payment Detail',
    data: { breadcrumb: 'Detail' },
    loadComponent: () => import('./payments/payment-detail-page'),
  },
  { path: '', pathMatch: 'full', redirectTo: 'payments' },
];
