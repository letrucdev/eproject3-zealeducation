import {
  FeeType,
  InstallmentFrequency,
  PaymentMethod,
  PaymentStatus,
  PaymentType,
} from './payment-enums';

export interface FeeStructureListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: PaymentStatus | null;
  type?: FeeType | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface SetPaymentTypePayload {
  paymentType: PaymentType;
  frequency: InstallmentFrequency | null;
}

export interface SetPaymentTypeInstallment {
  id: string;
  installmentNo: number;
  amountDue: number;
  dueDate: string;
}

export interface SetPaymentTypeResponse {
  paymentType: PaymentType;
  frequency: InstallmentFrequency | null;
  installments: SetPaymentTypeInstallment[];
}

export interface ConfirmPaymentPayload {
  installmentPlanId: string | null;
  paymentMethod: PaymentMethod;
  proofFile: File | null;
}

export interface ConfirmPaymentResponse {
  transactionId: string;
  receiptNumber: string;
  baseAmount: number;
  penaltyAmount: number;
  totalAmount: number;
  newPaymentStatus: PaymentStatus;
}
