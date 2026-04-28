import { CandidateStatus } from '@core/models/candidate-status';
import { EnrollmentStatus } from './candidate-detail';

export const CANDIDATE_STATUS_LABELS: Record<CandidateStatus, string> = {
  [CandidateStatus.Active]: 'Active',
  [CandidateStatus.OnBreak]: 'On Break',
  [CandidateStatus.Graduated]: 'Graduated',
  [CandidateStatus.Dropped]: 'Dropped',
};

export const ENROLLMENT_STATUS_LABELS: Record<EnrollmentStatus, string> = {
  [EnrollmentStatus.PendingAssignment]: 'Pending Assignment',
  [EnrollmentStatus.Enrolled]: 'Enrolled',
  [EnrollmentStatus.Completed]: 'Completed',
  [EnrollmentStatus.Withdrawn]: 'Withdrawn',
  [EnrollmentStatus.OnBreak]: 'On Break',
};
