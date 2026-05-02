import { EnrollmentStatus } from '@core/models/candidate-detail';

export interface BatchEnrollmentItem {
  enrollmentId: string;
  candidateId: string;
  candidateCode: string;
  fullName: string;
  email: string;
  phone: string;
  enrollmentDate: string;
  status: EnrollmentStatus;
}

export interface AssignableCandidate {
  enrollmentId: string;
  candidateId: string;
  candidateCode: string;
  fullName: string;
  email: string;
  phone: string;
  enrollmentDate: string;
}
