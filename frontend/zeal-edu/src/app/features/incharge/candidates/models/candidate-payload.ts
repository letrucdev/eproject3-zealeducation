import { CandidateStatus } from '@core/models/candidate-status';

export interface CandidateListQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: CandidateStatus | null;
  courseId?: string | null;
  batchId?: string | null;
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
