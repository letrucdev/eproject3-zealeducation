import { CandidateStatus } from './candidate-status';

export interface CandidateInfo {
  id: string;
  candidateCode: string;
  address: string | null;
  emergencyContact: string | null;
  status: CandidateStatus;
  registeredAt: string;
}
