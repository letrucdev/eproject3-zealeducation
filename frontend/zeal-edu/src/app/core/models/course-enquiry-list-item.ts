import { EnquirySource } from './enquiry-source';
import { EnquiryStatus } from './enquiry-status';

export interface CourseEnquiryListItem {
  enquiryId: string;
  fullName: string;
  phone: string;
  email: string | null;
  courseInterested: string;
  source: EnquirySource;
  status: EnquiryStatus;
  nextFollowUpDate: string | null;
  assignedCounselorId: string;
  assignedCounselorName: string;
  convertedCandidateId: string | null;
  convertedAt: string | null;
  createdAt: string;
}
