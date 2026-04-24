import { EnquiryNote } from './enquiry-note';
import { EnquirySource } from './enquiry-source';
import { EnquiryStatus } from './enquiry-status';

export interface CourseEnquiryDetail {
  enquiryId: string;
  fullName: string;
  phone: string;
  email: string | null;
  courseInterestedId: string;
  courseInterestedName: string;
  source: EnquirySource;
  status: EnquiryStatus;
  nextFollowUpDate: string | null;
  assignedCounselorId: string;
  assignedCounselorName: string;
  convertedCandidateId: string | null;
  convertedCandidateCode: string | null;
  convertedAt: string | null;
  createdAt: string;
  updatedAt: string | null;
  notes: EnquiryNote[];
}
