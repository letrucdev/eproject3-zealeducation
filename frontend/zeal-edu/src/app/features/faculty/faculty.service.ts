import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { MarkAttendancePayload, SessionAttendance } from '@core/models/attendance';
import { BatchDetail } from '@core/models/batch-detail';
import { BatchEnrollmentItem } from '@core/models/batch-enrollment';
import { BatchListItem } from '@core/models/batch-list-item';
import { ClassSession } from '@core/models/class-session';
import { ExamResult, ExamResultsQuery } from '@core/models/exam-result';
import { PaginatedList } from '@core/models/paginated-list';
import {
  FacultyBatchEnrollmentsQuery,
  FacultyBatchListQuery,
  FacultyCourseOption,
  FacultyCoursesQuery,
  FacultyExamResultPayload,
  FacultyExaminationCandidate,
  FacultyExaminationSummary,
  FacultyExaminationsQuery,
  FacultyScheduleItem,
  FacultyScheduleQuery,
  FacultyUpdateExamResultPayload,
} from './models/faculty-models';

export const FACULTY_BATCHES_KEY = ['faculty-batches'] as const;
export const FACULTY_BATCH_DETAIL_KEY = ['faculty-batch-detail'] as const;
export const FACULTY_BATCH_SESSIONS_KEY = ['faculty-batch-sessions'] as const;
export const FACULTY_BATCH_ENROLLMENTS_KEY = ['faculty-batch-enrollments'] as const;
export const FACULTY_SCHEDULE_KEY = ['faculty-schedule'] as const;
export const FACULTY_UPCOMING_SESSIONS_KEY = ['faculty-upcoming-sessions'] as const;
export const FACULTY_COURSES_KEY = ['faculty-courses'] as const;
export const FACULTY_SESSION_ATTENDANCE_KEY = ['faculty-session-attendance'] as const;
export const FACULTY_EXAMINATIONS_KEY = ['faculty-examinations'] as const;
export const FACULTY_EXAM_RESULTS_KEY = ['faculty-exam-results'] as const;
export const FACULTY_EXAM_CANDIDATES_KEY = ['faculty-exam-candidates'] as const;

const emptyPaginated = <T>(query: { page: number }): PaginatedList<T> => ({
  items: [],
  pageNumber: query.page,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
});

@Injectable({ providedIn: 'root' })
export class FacultyService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  batchesListQuery(params: Signal<FacultyBatchListQuery>) {
    return injectQuery<PaginatedList<BatchListItem>, HttpErrorResponse>(() => ({
      queryKey: [...FACULTY_BATCHES_KEY, params()],
      queryFn: () => this._fetchBatches(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  batchDetailQuery(batchId: Signal<string | null>) {
    return injectQuery<BatchDetail, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...FACULTY_BATCH_DETAIL_KEY, batchId()],
      queryFn: () => this._fetchBatchDetail(batchId() as string),
    }));
  }

  batchSessionsQuery(
    batchId: Signal<string | null>,
    params: Signal<{ page: number; pageSize: number; sortBy?: string; sortDirection?: 'asc' | 'desc' }>,
  ) {
    return injectQuery<PaginatedList<ClassSession>, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...FACULTY_BATCH_SESSIONS_KEY, batchId(), params()],
      queryFn: () => this._fetchBatchSessions(batchId() as string, params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  batchEnrollmentsQuery(
    batchId: Signal<string | null>,
    params: Signal<FacultyBatchEnrollmentsQuery>,
  ) {
    return injectQuery<PaginatedList<BatchEnrollmentItem>, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...FACULTY_BATCH_ENROLLMENTS_KEY, batchId(), params()],
      queryFn: () => this._fetchBatchEnrollments(batchId() as string, params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  scheduleQuery(params: Signal<FacultyScheduleQuery>) {
    return injectQuery<PaginatedList<FacultyScheduleItem>, HttpErrorResponse>(() => ({
      queryKey: [...FACULTY_SCHEDULE_KEY, params()],
      queryFn: () => this._fetchSchedule(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  upcomingSessionsQuery() {
    return injectQuery<FacultyScheduleItem[], HttpErrorResponse>(() => ({
      queryKey: FACULTY_UPCOMING_SESSIONS_KEY,
      queryFn: () => this._fetchUpcomingSessions(),
      staleTime: 30_000,
      refetchInterval: 60_000,
    }));
  }

  coursesQuery(params: Signal<FacultyCoursesQuery>) {
    return injectQuery<PaginatedList<FacultyCourseOption>, HttpErrorResponse>(() => ({
      queryKey: [...FACULTY_COURSES_KEY, params()],
      queryFn: () => this._fetchCourses(params()),
      staleTime: 60_000,
      placeholderData: keepPreviousData,
    }));
  }

  sessionAttendanceQuery(sessionId: Signal<string | null>) {
    return injectQuery<SessionAttendance, HttpErrorResponse>(() => ({
      enabled: sessionId() !== null,
      queryKey: [...FACULTY_SESSION_ATTENDANCE_KEY, sessionId()],
      queryFn: () => this._fetchSessionAttendance(sessionId() as string),
    }));
  }

  examinationsQuery(params: Signal<FacultyExaminationsQuery>) {
    return injectQuery<PaginatedList<FacultyExaminationSummary>, HttpErrorResponse>(() => ({
      queryKey: [...FACULTY_EXAMINATIONS_KEY, params()],
      queryFn: () => this._fetchExaminations(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  examResultsQuery(
    examinationId: Signal<string | null>,
    params: Signal<ExamResultsQuery>,
    enabled: Signal<boolean>,
  ) {
    return injectQuery<PaginatedList<ExamResult>, HttpErrorResponse>(() => ({
      enabled: enabled() && examinationId() !== null,
      queryKey: [...FACULTY_EXAM_RESULTS_KEY, examinationId(), params()],
      queryFn: () => this._fetchExamResults(examinationId() as string, params()),
      staleTime: 15_000,
      placeholderData: keepPreviousData,
    }));
  }

  examCandidatesQuery(examinationId: Signal<string | null>, enabled: Signal<boolean>) {
    return injectQuery<FacultyExaminationCandidate[], HttpErrorResponse>(() => ({
      enabled: enabled() && examinationId() !== null,
      queryKey: [...FACULTY_EXAM_CANDIDATES_KEY, examinationId()],
      queryFn: () => this._fetchExamCandidates(examinationId() as string),
      staleTime: 15_000,
    }));
  }

  markAttendanceMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { sessionId: string; payload: MarkAttendancePayload }
    >(() => ({
      mutationFn: ({ sessionId, payload }) =>
        firstValueFrom(
          this._http.post<ApiResponse<unknown>>(
            `/faculty/me/sessions/${sessionId}/attendance`,
            payload,
          ),
        ),
      onSuccess: () => {
        void this._queryClient.invalidateQueries({ queryKey: FACULTY_SESSION_ATTENDANCE_KEY });
        void this._queryClient.invalidateQueries({ queryKey: FACULTY_BATCH_SESSIONS_KEY });
        void this._queryClient.invalidateQueries({ queryKey: FACULTY_SCHEDULE_KEY });
      },
    }));
  }

  createExamResultMutation() {
    return injectMutation<
      string,
      HttpErrorResponse,
      { examinationId: string; payload: FacultyExamResultPayload }
    >(() => ({
      mutationFn: async ({ examinationId, payload }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<string>>(
            `/examinations/${examinationId}/results`,
            payload,
          ),
        );
        if (!response.data) throw new Error(response.message || 'Failed to record exam result');
        return response.data;
      },
      onSuccess: () => this._invalidateExams(),
    }));
  }

  updateExamResultMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { resultId: string; payload: FacultyUpdateExamResultPayload }
    >(() => ({
      mutationFn: ({ resultId, payload }) =>
        firstValueFrom(
          this._http.put<ApiResponse<unknown>>(`/exam-results/${resultId}`, payload),
        ),
      onSuccess: () => this._invalidateExams(),
    }));
  }

  private async _fetchBatches(query: FacultyBatchListQuery): Promise<PaginatedList<BatchListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.status) params = params.set('status', query.status);
    if (query.courseId) params = params.set('courseId', query.courseId);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<BatchListItem>>>('/faculty/me/batches', { params }),
    );
    return response.data ?? emptyPaginated<BatchListItem>(query);
  }

  private async _fetchBatchDetail(batchId: string): Promise<BatchDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<BatchDetail>>(`/faculty/me/batches/${batchId}`),
    );
    if (!response.data) throw new Error(response.message || 'Batch not found');
    return response.data;
  }

  private async _fetchBatchSessions(
    batchId: string,
    query: { page: number; pageSize: number; sortBy?: string; sortDirection?: 'asc' | 'desc' },
  ): Promise<PaginatedList<ClassSession>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<ClassSession>>>(
        `/faculty/me/batches/${batchId}/sessions`,
        { params },
      ),
    );
    return response.data ?? emptyPaginated<ClassSession>(query);
  }

  private async _fetchBatchEnrollments(
    batchId: string,
    query: FacultyBatchEnrollmentsQuery,
  ): Promise<PaginatedList<BatchEnrollmentItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<BatchEnrollmentItem>>>(
        `/faculty/me/batches/${batchId}/enrollments`,
        { params },
      ),
    );
    return response.data ?? emptyPaginated<BatchEnrollmentItem>(query);
  }

  private async _fetchSchedule(query: FacultyScheduleQuery): Promise<PaginatedList<FacultyScheduleItem>> {
    let params = new HttpParams()
      .set('from', query.from)
      .set('to', query.to)
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.batchId) params = params.set('batchId', query.batchId);
    if (query.courseId) params = params.set('courseId', query.courseId);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FacultyScheduleItem>>>('/faculty/me/schedule', { params }),
    );
    return response.data ?? emptyPaginated<FacultyScheduleItem>(query);
  }

  private async _fetchUpcomingSessions(): Promise<FacultyScheduleItem[]> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<FacultyScheduleItem[]>>('/faculty/me/upcoming-sessions'),
    );
    return response.data ?? [];
  }

  private async _fetchCourses(query: FacultyCoursesQuery): Promise<PaginatedList<FacultyCourseOption>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FacultyCourseOption>>>('/faculty/me/courses', { params }),
    );
    return response.data ?? emptyPaginated<FacultyCourseOption>(query);
  }

  private async _fetchSessionAttendance(sessionId: string): Promise<SessionAttendance> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<SessionAttendance>>(
        `/faculty/me/sessions/${sessionId}/attendance`,
      ),
    );
    if (!response.data) throw new Error(response.message || 'Session attendance not found');
    return response.data;
  }

  private async _fetchExaminations(
    query: FacultyExaminationsQuery,
  ): Promise<PaginatedList<FacultyExaminationSummary>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.batchId) params = params.set('batchId', query.batchId);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FacultyExaminationSummary>>>(
        '/faculty/me/examinations',
        { params },
      ),
    );
    return response.data ?? emptyPaginated<FacultyExaminationSummary>(query);
  }

  private async _fetchExamResults(
    examinationId: string,
    query: ExamResultsQuery,
  ): Promise<PaginatedList<ExamResult>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<ExamResult>>>(
        `/examinations/${examinationId}/results`,
        { params },
      ),
    );
    return response.data ?? emptyPaginated<ExamResult>(query);
  }

  private async _fetchExamCandidates(
    examinationId: string,
  ): Promise<FacultyExaminationCandidate[]> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<FacultyExaminationCandidate[]>>(
        `/faculty/me/examinations/${examinationId}/candidates`,
      ),
    );
    return response.data ?? [];
  }

  private _invalidateExams(): void {
    void this._queryClient.invalidateQueries({ queryKey: FACULTY_EXAM_RESULTS_KEY });
    void this._queryClient.invalidateQueries({ queryKey: FACULTY_EXAMINATIONS_KEY });
    void this._queryClient.invalidateQueries({ queryKey: FACULTY_EXAM_CANDIDATES_KEY });
  }
}
