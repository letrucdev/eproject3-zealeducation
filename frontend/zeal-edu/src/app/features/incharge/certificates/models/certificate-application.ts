export type CertificateApplicationStatus = 'Pending' | 'Approved';

export interface CertificateApplicationListItem {
  applicationId: string;
  candidateId: string;
  candidateCode: string;
  candidateName: string;
  batchId: string | null;
  batchCode: string | null;
  courseId: string;
  courseName: string;
  status: CertificateApplicationStatus;
  certificateNumber: string | null;
  appliedAt: string;
  approvedAt: string | null;
  approvedByStaffId: string | null;
  approvedByName: string | null;

  feesPaid: boolean;
  attendancePercent: number;
  attendanceOk: boolean;
  examsPassed: boolean;
  isCurrentlyEligible: boolean;
}

export interface CertificateApplicationListQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: CertificateApplicationStatus | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}
