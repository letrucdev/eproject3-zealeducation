export type CertificateApplicationStatus = 'Pending' | 'Approved';

export interface MyCertificateListItem {
  applicationId: string;
  batchId: string | null;
  batchCode: string | null;
  courseId: string;
  courseName: string;
  status: CertificateApplicationStatus;
  certificateNumber: string | null;
  appliedAt: string;
  approvedAt: string | null;
}

export interface MyCertificateEligibility {
  batchId: string;
  isEligible: boolean;
  feesPaid: boolean;
  attendancePercent: number;
  attendanceOk: boolean;
  examsPassed: boolean;
  reason: string | null;
  existingApplicationStatus: CertificateApplicationStatus | null;
}

export interface ApplyForCertificatePayload {
  batchId: string;
}
