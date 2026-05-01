import { ClassSession, ClassSessionStatus } from '@core/models/class-session';
import { AttendanceStatus } from '@core/models/attendance';
import { BatchStatus } from '@core/models/batch-status';
import { PaginatedList } from '@core/models/paginated-list';
import { EnrollmentStatus } from '@core/models/candidate-detail';

export interface MyBatchListItem {
  enrollmentId: string;
  batchId: string;
  batchCode: string;
  courseId: string;
  courseName: string;
  facultyId: string | null;
  facultyName: string | null;
  startDate: string;
  endDate: string;
  location: string | null;
  batchStatus: BatchStatus;
  enrollmentStatus: EnrollmentStatus;
  enrollmentDate: string;
}

export interface MyBatchListQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface MyBatchSessionsQuery {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  fromDate?: string;
  toDate?: string;
}

export interface MyBatchAttendanceQuery {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  fromDate?: string;
  toDate?: string;
}

export interface MyBatchFeedbackState {
  courseSubmitted: boolean;
  generalSubmitted: boolean;
  facultyTargetsSubmitted: string[];
}

export interface MyBatchDetail {
  batchId: string;
  batchCode: string;
  courseId: string;
  courseName: string;
  courseDurationWeeks: number;
  facultyId: string | null;
  facultyName: string | null;
  facultyCode: string | null;
  startDate: string;
  endDate: string;
  location: string | null;
  maxCapacity: number;
  enrolledCount: number;
  status: BatchStatus;
  enrollmentId: string;
  enrollmentStatus: EnrollmentStatus;
  feedbackState: MyBatchFeedbackState;
}

export interface MyAttendanceRow {
  sessionId: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
  sessionStatus: ClassSessionStatus;
  attendanceStatus: AttendanceStatus | null;
  practicalHours: number | null;
  remarks: string | null;
}

export interface MyBatchAttendance {
  batchId: string;
  enrollmentId: string;
  totalPracticalHours: number;
  totalSessions: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  rows: PaginatedList<MyAttendanceRow>;
}

export interface MyExamResultRow {
  examId: string;
  examName: string;
  examDate: string;
  maxScore: number;
  passScore: number;
  location: string | null;
  resultId: string | null;
  score: number | null;
  grade: string | null;
  isPassed: boolean | null;
  isOverridden: boolean;
  gradedAt: string | null;
}

export interface MyBatchExamResults {
  batchId: string;
  enrollmentId: string;
  rows: MyExamResultRow[];
}

export type MyBatchSession = ClassSession;
