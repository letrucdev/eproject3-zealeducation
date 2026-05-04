import {
  FeeType,
  InstallmentStatus,
  PaymentMethod,
  PaymentStatus,
  PaymentType,
} from '@core/models/payment-enums';

export interface FeeStructureListItem {
  feeId: string;
  candidateId: string;
  candidateCode: string;
  candidateFullName: string;
  courseTitle: string | null;
  feeType: FeeType;
  totalFee: number;
  amountPaid: number;
  outstandingBalance: number;
  paymentStatus: PaymentStatus;
  paymentType: PaymentType;
  createdAt: string;
}

export interface InstallmentPlanItem {
  id: string;
  installmentNo: number;
  amountDue: number;
  dueDate: string;
  amountPaid: number;
  paidDate: string | null;
  status: InstallmentStatus;
  penaltyAmount: number;
}

export interface PaymentTransactionItem {
  id: string;
  installmentPlanId: string | null;
  installmentNo: number | null;
  receiptNumber: string;
  paymentDate: string;
  amount: number;
  paymentMethod: PaymentMethod;
  processedByStaffName: string | null;
  hasBankTransferProof: boolean;
}

export interface FeeStructureDetail {
  feeId: string;
  candidateId: string;
  candidateCode: string;
  candidateFullName: string;
  candidateEmail: string | null;
  candidatePhone: string | null;
  candidateIsActive: boolean;
  enrollmentId: string | null;
  enrollmentDate: string | null;
  courseId: string | null;
  courseName: string | null;
  durationWeeks: number | null;
  feeType: FeeType;
  totalFee: number;
  penaltyApplied: number;
  projectedPenalty: number;
  adjustedTotalFee: number;
  amountPaid: number;
  outstandingBalance: number;
  paymentStatus: PaymentStatus;
  paymentType: PaymentType;
  notes: string | null;
  installmentPlans: InstallmentPlanItem[];
  paymentTransactions: PaymentTransactionItem[];
}
