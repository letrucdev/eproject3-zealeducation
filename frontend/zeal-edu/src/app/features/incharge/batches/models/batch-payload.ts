import { BatchStatus } from '@core/models/batch-status';

export interface BatchListQuery {
  page: number;
  pageSize: number;
  search?: string;
  courseId?: string | null;
  facultyId?: string | null;
  status?: BatchStatus | null;
}

export interface CreateBatchPayload {
  batchCode: string;
  courseId: string;
  facultyId: string | null;
  startDate: string;
  endDate: string;
  location: string | null;
  maxCapacity: number;
}

export interface UpdateBatchPayload {
  batchCode: string;
  courseId: string;
  startDate: string;
  endDate: string;
  location: string | null;
  maxCapacity: number;
  status: BatchStatus;
}

export interface AssignFacultyPayload {
  facultyId: string | null;
}

export interface BatchEnrollmentsQuery {
  page: number;
  pageSize: number;
  search?: string;
}

export interface AssignableCandidatesQuery {
  page: number;
  pageSize: number;
  search?: string;
}

export interface AssignCandidatesPayload {
  enrollmentIds: string[];
}
