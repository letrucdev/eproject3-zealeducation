import { EnquirySource } from '../../../../core/models/enquiry-source';
import { EnquiryStatus } from '../../../../core/models/enquiry-status';

export const ENQUIRY_STATUS_LABELS: Record<EnquiryStatus, string> = {
  [EnquiryStatus.New]: 'New',
  [EnquiryStatus.Contacted]: 'Contacted',
  [EnquiryStatus.InFollowUp]: 'In Follow-Up',
  [EnquiryStatus.Interested]: 'Interested',
  [EnquiryStatus.Converted]: 'Converted',
  [EnquiryStatus.Closed]: 'Closed',
};

export const ENQUIRY_SOURCE_LABELS: Record<EnquirySource, string> = {
  [EnquirySource.WalkIn]: 'Walk-in',
  [EnquirySource.Phone]: 'Phone',
  [EnquirySource.Website]: 'Website',
  [EnquirySource.Referral]: 'Referral',
  [EnquirySource.SocialMedia]: 'Social media',
  [EnquirySource.Other]: 'Other',
};
