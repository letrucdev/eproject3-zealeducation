import { CandidateStatus } from '@core/models/candidate-status';

export interface CandidateListQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: CandidateStatus | null;
  courseId?: string | null;
  batchId?: string | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface UpdateCandidatePayload {
  fullName: string;
  email: string;
  phone: string;
  address: string | null;
  emergencyContact: string | null;
  notes: string | null;
  status: CandidateStatus;
}

export interface ApplyFinePayload {
  violationReason: string;
  penaltyAmount: number;
}

export interface ApplyFineResponse {
  fineId: string;
  feeId: string;
}

export interface ResetCandidatePasswordResponse {
  username: string;
  temporaryPassword: string;
}

export interface AddEnrollmentPayload {
  courseId: string;
}

export interface AddEnrollmentResponse {
  enrollmentId: string;
  feeId: string;
  courseName: string;
  totalFee: number;
}
