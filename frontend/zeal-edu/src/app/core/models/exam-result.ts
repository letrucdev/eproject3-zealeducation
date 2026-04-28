export interface ExamResult {
  resultId: string;
  examId: string;
  enrollmentId: string;
  candidateCode: string;
  candidateFullName: string;
  score: number;
  grade: string | null;
  isPassed: boolean;
  gradedById: string;
  gradedByName: string;
  isOverridden: boolean;
  overrideById: string | null;
  overrideByName: string | null;
  overrideReason: string | null;
  gradedAt: string;
}

export interface OverrideExamResultPayload {
  score: number;
  grade: string | null;
  overrideReason: string;
}

export interface ExamResultsQuery {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}
