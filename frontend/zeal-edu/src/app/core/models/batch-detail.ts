import { BatchStatus } from './batch-status';

export interface BatchDetail {
  batchId: string;
  batchCode: string;
  courseId: string;
  courseName: string;
  facultyId: string | null;
  facultyName: string | null;
  facultyCode: string | null;
  startDate: string;
  endDate: string;
  location: string | null;
  maxCapacity: number;
  enrolledCount: number;
  sessionCount: number;
  status: BatchStatus;
  createdAt: string;
  updatedAt: string | null;
}
