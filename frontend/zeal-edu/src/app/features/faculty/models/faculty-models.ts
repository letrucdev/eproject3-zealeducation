import { BatchStatus } from '@core/models/batch-status';
import { ClassSessionStatus } from '@core/models/class-session';

export interface FacultyBatchListQuery {
  page: number;
  pageSize: number;
  search?: string;
  status?: BatchStatus | null;
  courseId?: string | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface FacultyExaminationsQuery {
  page: number;
  pageSize: number;
  search?: string;
  batchId?: string | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface FacultyBatchEnrollmentsQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface FacultyScheduleQuery {
  from: string;
  to: string;
  page: number;
  pageSize: number;
  batchId?: string | null;
  courseId?: string | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface FacultyCoursesQuery {
  page: number;
  pageSize: number;
  search?: string;
}

export interface FacultyCourseOption {
  courseId: string;
  courseName: string;
}

export interface FacultyScheduleItem {
  sessionId: string;
  batchId: string;
  batchCode: string;
  courseName: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
  status: ClassSessionStatus;
}

export interface FacultyExaminationSummary {
  examinationId: string;
  batchId: string;
  batchCode: string;
  courseName: string;
  examName: string;
  examDate: string;
  location: string | null;
  maxScore: number;
  passScore: number;
  resultCount: number;
  averageScore: number | null;
  minScore: number | null;
  maxStudentScore: number | null;
  createdAt: string;
}

export interface FacultyExamResultPayload {
  enrollmentId: string;
  score: number;
  isFinalized: boolean;
}

export interface FacultyUpdateExamResultPayload {
  score: number;
  isFinalized: boolean;
}

export interface FacultyExaminationCandidate {
  enrollmentId: string;
  candidateId: string;
  candidateCode: string;
  candidateFullName: string;
  resultId: string | null;
  score: number | null;
  grade: string | null;
  isPassed: boolean | null;
  isFinalized: boolean;
  isOverridden: boolean;
  gradedAt: string | null;
}
