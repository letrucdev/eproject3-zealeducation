import { CandidateStatus } from '@core/models/candidate-status';

export interface CandidateListItem {
  candidateId: string;
  candidateCode: string;
  fullName: string;
  email: string;
  phone: string;
  isActive: boolean;
  status: CandidateStatus;
  registeredAt: string;
  currentEnrollmentId: string | null;
  currentCourseId: string | null;
  currentCourseName: string | null;
  currentBatchId: string | null;
  currentBatchCode: string | null;
}
