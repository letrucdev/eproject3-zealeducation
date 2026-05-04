import { FeeType, PaymentMethod, PaymentStatus } from '@core/models/payment-enums';

export interface RevenueTrendPoint {
  date: string;
  amount: number;
  count: number;
}

export interface PaymentStatusBucket {
  status: PaymentStatus;
  count: number;
  outstandingAmount: number;
}

export interface FeeTypeRevenue {
  feeType: FeeType;
  amount: number;
  count: number;
}

export interface CourseRevenue {
  courseId: string;
  courseTitle: string;
  amount: number;
  transactionCount: number;
}

export interface FinancialReport {
  from: string;
  to: string;
  monthlyProfit: number;
  yearlyIncome: number;
  outstanding: number;
  transactionCount: number;
  revenueTrend: RevenueTrendPoint[];
  paymentStatusDistribution: PaymentStatusBucket[];
  revenueByFeeType: FeeTypeRevenue[];
  topCoursesByRevenue: CourseRevenue[];
}

export interface FinancialTransactionListItem {
  transactionId: string;
  feeId: string;
  receiptNumber: string;
  paymentDate: string;
  candidateCode: string;
  candidateFullName: string;
  courseTitle: string | null;
  feeType: FeeType;
  amount: number;
  paymentMethod: PaymentMethod;
  processedByStaffName: string;
}

export interface FinancialReportRange {
  from: string;
  to: string;
}

export interface FinancialTransactionsQuery extends FinancialReportRange {
  page?: number;
  pageSize?: number;
  search?: string;
  feeType?: FeeType | null;
  method?: PaymentMethod | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface FinancialReportExportParams {
  revenueTrend: FinancialReportRange;
  paymentStatus: FinancialReportRange;
  revenueByFeeType: FinancialReportRange;
  topCourses: FinancialReportRange;
  transactions: FinancialReportRange;
  search?: string;
  feeType?: FeeType | null;
  method?: PaymentMethod | null;
}
