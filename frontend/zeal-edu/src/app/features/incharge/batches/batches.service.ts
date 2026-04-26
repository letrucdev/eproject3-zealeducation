import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { AssignableCandidate, BatchEnrollmentItem } from '@core/models/batch-enrollment';
import { BatchDetail } from '@core/models/batch-detail';
import { BatchListItem } from '@core/models/batch-list-item';
import { BatchStatistics } from '@core/models/batch-statistics';
import {
  ClassSession,
  CreateClassSessionPayload,
  UpdateClassSessionPayload,
} from '@core/models/class-session';
import { MarkAttendancePayload, SessionAttendance } from '@core/models/attendance';
import { PaginatedList } from '@core/models/paginated-list';
import {
  AssignCandidatesPayload,
  AssignFacultyPayload,
  AssignableCandidatesQuery,
  BatchEnrollmentsQuery,
  BatchListQuery,
  CreateBatchPayload,
  UpdateBatchPayload,
} from './models/batch-payload';

export const BATCH_QUERY_KEY = ['batches'] as const;
export const BATCH_DETAIL_QUERY_KEY = ['batch-detail'] as const;
export const BATCH_STATS_QUERY_KEY = ['batch-statistics'] as const;
export const BATCH_ENROLLMENTS_QUERY_KEY = ['batch-enrollments'] as const;
export const BATCH_ASSIGNABLE_QUERY_KEY = ['batch-assignable'] as const;
export const BATCH_SESSIONS_QUERY_KEY = ['batch-sessions'] as const;
export const SESSION_ATTENDANCE_QUERY_KEY = ['session-attendance'] as const;

interface CreateBatchResponse {
  batchId: string;
  batchCode: string;
  status: string;
}

const emptyPaginated = <T>(query: { page: number }): PaginatedList<T> => ({
  items: [],
  pageNumber: query.page,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
});

@Injectable({ providedIn: 'root' })
export class BatchesService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<BatchListQuery>) {
    return injectQuery(() => ({
      queryKey: [...BATCH_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  statisticsQuery() {
    return injectQuery(() => ({
      queryKey: BATCH_STATS_QUERY_KEY,
      queryFn: () => this._fetchStatistics(),
    }));
  }

  detailQuery(batchId: Signal<string | null>) {
    return injectQuery<BatchDetail, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...BATCH_DETAIL_QUERY_KEY, batchId()],
      queryFn: () => this._fetchDetail(batchId() as string),
    }));
  }

  enrollmentsQuery(batchId: Signal<string | null>, params: Signal<BatchEnrollmentsQuery>) {
    return injectQuery<PaginatedList<BatchEnrollmentItem>, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...BATCH_ENROLLMENTS_QUERY_KEY, batchId(), params()],
      queryFn: () => this._fetchEnrollments(batchId() as string, params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  assignableCandidatesQuery(
    batchId: Signal<string | null>,
    params: Signal<AssignableCandidatesQuery>,
    enabled: Signal<boolean>,
  ) {
    return injectQuery<PaginatedList<AssignableCandidate>, HttpErrorResponse>(() => ({
      enabled: enabled() && batchId() !== null,
      queryKey: [...BATCH_ASSIGNABLE_QUERY_KEY, batchId(), params()],
      queryFn: () => this._fetchAssignable(batchId() as string, params()),
      staleTime: 10_000,
      placeholderData: keepPreviousData,
    }));
  }

  sessionsQuery(batchId: Signal<string | null>) {
    return injectQuery<ClassSession[], HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...BATCH_SESSIONS_QUERY_KEY, batchId()],
      queryFn: () => this._fetchSessions(batchId() as string),
    }));
  }

  sessionAttendanceQuery(sessionId: Signal<string | null>) {
    return injectQuery<SessionAttendance, HttpErrorResponse>(() => ({
      enabled: sessionId() !== null,
      queryKey: [...SESSION_ATTENDANCE_QUERY_KEY, sessionId()],
      queryFn: () => this._fetchSessionAttendance(sessionId() as string),
    }));
  }

  createMutation() {
    return injectMutation<CreateBatchResponse, HttpErrorResponse, CreateBatchPayload>(() => ({
      mutationFn: async (payload) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<CreateBatchResponse>>('/batches', payload),
        );
        if (!response.data) throw new Error(response.message || 'Failed to create batch');
        return response.data;
      },
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { batchId: string; payload: UpdateBatchPayload }
    >(() => ({
      mutationFn: ({ batchId, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/batches/${batchId}`, payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  assignFacultyMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { batchId: string; payload: AssignFacultyPayload }
    >(() => ({
      mutationFn: ({ batchId, payload }) =>
        firstValueFrom(
          this._http.patch<ApiResponse<unknown>>(`/batches/${batchId}/faculty`, payload),
        ),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  deleteMutation() {
    return injectMutation<unknown, HttpErrorResponse, string>(() => ({
      mutationFn: (batchId) =>
        firstValueFrom(this._http.delete<ApiResponse<unknown>>(`/batches/${batchId}`)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  assignCandidatesMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { batchId: string; payload: AssignCandidatesPayload }
    >(() => ({
      mutationFn: ({ batchId, payload }) =>
        firstValueFrom(
          this._http.post<ApiResponse<unknown>>(`/batches/${batchId}/candidates`, payload),
        ),
      onSuccess: () => {
        this._invalidateAll();
      },
    }));
  }

  createSessionMutation() {
    return injectMutation<
      string,
      HttpErrorResponse,
      { batchId: string; payload: CreateClassSessionPayload }
    >(() => ({
      mutationFn: async ({ batchId, payload }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<string>>(`/batches/${batchId}/sessions`, payload),
        );
        if (!response.data) throw new Error(response.message || 'Failed to create session');
        return response.data;
      },
      onSuccess: () => {
        void this._queryClient.invalidateQueries({ queryKey: BATCH_SESSIONS_QUERY_KEY });
      },
    }));
  }

  updateSessionMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { sessionId: string; payload: UpdateClassSessionPayload }
    >(() => ({
      mutationFn: ({ sessionId, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/sessions/${sessionId}`, payload)),
      onSuccess: () => {
        this._invalidateAll();
      },
    }));
  }

  deleteSessionMutation() {
    return injectMutation<unknown, HttpErrorResponse, string>(() => ({
      mutationFn: (sessionId) =>
        firstValueFrom(this._http.delete<ApiResponse<unknown>>(`/sessions/${sessionId}`)),
      onSuccess: () => {
        this._invalidateAll();
      },
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
          this._http.post<ApiResponse<unknown>>(`/sessions/${sessionId}/attendance`, payload),
        ),
      onSuccess: () => {
        this._invalidateAll();
      },
    }));
  }

  private async _fetchList(query: BatchListQuery): Promise<PaginatedList<BatchListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.courseId) params = params.set('courseId', query.courseId);
    if (query.facultyId) params = params.set('facultyId', query.facultyId);
    if (query.status) params = params.set('status', query.status);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<BatchListItem>>>('/batches', { params }),
    );
    return response.data ?? emptyPaginated<BatchListItem>(query);
  }

  private async _fetchStatistics(): Promise<BatchStatistics> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<BatchStatistics>>('/batches/statistics'),
    );
    return (
      response.data ?? {
        total: 0,
        needsInstructor: 0,
        active: 0,
        completed: 0,
        cancelled: 0,
      }
    );
  }

  private async _fetchDetail(batchId: string): Promise<BatchDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<BatchDetail>>(`/batches/${batchId}`),
    );
    if (!response.data) throw new Error(response.message || 'Batch not found');
    return response.data;
  }

  private async _fetchEnrollments(
    batchId: string,
    query: BatchEnrollmentsQuery,
  ): Promise<PaginatedList<BatchEnrollmentItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<BatchEnrollmentItem>>>(
        `/batches/${batchId}/enrollments`,
        { params },
      ),
    );
    return response.data ?? emptyPaginated<BatchEnrollmentItem>(query);
  }

  private async _fetchAssignable(
    batchId: string,
    query: AssignableCandidatesQuery,
  ): Promise<PaginatedList<AssignableCandidate>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<AssignableCandidate>>>(
        `/batches/${batchId}/assignable-candidates`,
        { params },
      ),
    );
    return response.data ?? emptyPaginated<AssignableCandidate>(query);
  }

  private async _fetchSessions(batchId: string): Promise<ClassSession[]> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<ClassSession[]>>(`/batches/${batchId}/sessions`),
    );
    return response.data ?? [];
  }

  private async _fetchSessionAttendance(sessionId: string): Promise<SessionAttendance> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<SessionAttendance>>(`/sessions/${sessionId}/attendance`),
    );
    if (!response.data) throw new Error(response.message || 'Session attendance not found');
    return response.data;
  }

  private _invalidateAll(): void {
    void this._queryClient.invalidateQueries({ queryKey: BATCH_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: BATCH_STATS_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: BATCH_DETAIL_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: BATCH_ENROLLMENTS_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: BATCH_ASSIGNABLE_QUERY_KEY });
  }
}
