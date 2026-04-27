import { BatchStatus } from '@core/models/batch-status';

export interface BatchListQuery {
  page: number;
  pageSize: number;
  search?: string;
  courseId?: string | null;
  facultyId?: string | null;
  status?: BatchStatus | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
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
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface BatchSessionsQuery {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface AssignableCandidatesQuery {
  page: number;
  pageSize: number;
  search?: string;
}

export interface AssignCandidatesPayload {
  enrollmentIds: string[];
}

export interface CreateBulkSessionsPayload {
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
}

export interface CreateBulkSessionsResponse {
  createdCount: number;
  skippedDates: string[];
}
