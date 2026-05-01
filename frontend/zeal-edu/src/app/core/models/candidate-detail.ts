import { CandidateStatus } from '@core/models/candidate-status';
import { Gender } from '@core/models/gender';
import { FeeType, PaymentStatus, PaymentType } from '@core/models/payment-enums';

export enum EnrollmentStatus {
  PendingAssignment = 'PendingAssignment',
  Enrolled = 'Enrolled',
  Completed = 'Completed',
  Withdrawn = 'Withdrawn',
  OnBreak = 'OnBreak',
}

export interface CandidateEnrollmentItem {
  enrollmentId: string;
  courseId: string;
  courseName: string;
  durationWeeks: number;
  batchId: string | null;
  batchCode: string | null;
  batchStartDate: string | null;
  batchEndDate: string | null;
  enrollmentDate: string;
  status: EnrollmentStatus;
  feeId: string | null;
  notes: string | null;
}

export interface CandidateFeeStructureSummary {
  feeId: string;
  feeType: FeeType;
  totalFee: number;
  amountPaid: number;
  outstandingBalance: number;
  paymentStatus: PaymentStatus;
  paymentType: PaymentType;
  courseTitle: string | null;
  notes: string | null;
  createdAt: string;
}

export interface CandidateDetail {
  candidateId: string;
  candidateCode: string;
  fullName: string;
  email: string;
  phone: string;
  dob: string;
  gender: Gender;
  isActive: boolean;
  address: string | null;
  emergencyContact: string | null;
  notes: string | null;
  status: CandidateStatus;
  registeredAt: string;
  enrollments: CandidateEnrollmentItem[];
  feeStructures: CandidateFeeStructureSummary[];
}
