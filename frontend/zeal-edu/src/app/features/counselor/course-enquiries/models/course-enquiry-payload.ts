import { Gender } from '@core/models/gender';
import { EnquirySource } from '@core/models/enquiry-source';
import { EnquiryStatus } from '@core/models/enquiry-status';

export interface CreateEnquiryPayload {
  fullName: string;
  phone: string;
  email: string | null;
  courseInterestedId: string;
  source: EnquirySource;
  status: EnquiryStatus;
  nextFollowUpDate: string | null;
}

export interface UpdateEnquiryPayload {
  fullName: string;
  phone: string;
  email: string | null;
  courseInterestedId: string;
  source: EnquirySource;
  status: EnquiryStatus;
  nextFollowUpDate: string | null;
}

export interface AddEnquiryNotePayload {
  content: string;
}

export interface ConvertEnquiryPayload {
  email: string;
  dob: string;
  gender: Gender;
  address: string | null;
  emergencyContact: string | null;
}

export interface EnquiryListQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: EnquiryStatus;
  source?: EnquirySource;
  dueFollowUpOnly?: boolean;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}
