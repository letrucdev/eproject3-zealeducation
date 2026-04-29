import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { parseFileNameFromContentDisposition } from '@core/http/content-disposition';
import { ClassSession } from '@core/models/class-session';
import { PaginatedList } from '@core/models/paginated-list';
import { CandidateDetail } from '@features/incharge/candidates/models/candidate-detail';
import {
  CourseMaterialsQuery,
  StudyMaterialListItem,
} from '@features/incharge/materials/models/material-payload';
import {
  MyBatchAttendance,
  MyBatchAttendanceQuery,
  MyBatchDetail,
  MyBatchExamResults,
  MyBatchListItem,
  MyBatchListQuery,
  MyBatchSessionsQuery,
} from './models/candidate-portal-models';

export const MY_PROFILE_QUERY_KEY = ['my-profile'] as const;
export const MY_BATCHES_QUERY_KEY = ['my-batches'] as const;
export const MY_BATCH_DETAIL_QUERY_KEY = ['my-batch-detail'] as const;
export const MY_BATCH_SESSIONS_QUERY_KEY = ['my-batch-sessions'] as const;
export const MY_BATCH_ATTENDANCE_QUERY_KEY = ['my-batch-attendance'] as const;
export const MY_BATCH_EXAM_RESULTS_QUERY_KEY = ['my-batch-exam-results'] as const;
export const MY_BATCH_MATERIALS_QUERY_KEY = ['my-batch-materials'] as const;

const emptyPaginated = <T>(query: { page: number }): PaginatedList<T> => ({
  items: [],
  pageNumber: query.page,
  totalPages: 0,
  totalCount: 0,
  hasPreviousPage: false,
  hasNextPage: false,
});

@Injectable({ providedIn: 'root' })
export class CandidatePortalService {
  private readonly _http = inject(HttpClient);

  profileQuery() {
    return injectQuery<CandidateDetail, HttpErrorResponse>(() => ({
      queryKey: MY_PROFILE_QUERY_KEY,
      queryFn: () => this._fetchProfile(),
      staleTime: 60_000,
    }));
  }

  batchesQuery(params: Signal<MyBatchListQuery>) {
    return injectQuery<PaginatedList<MyBatchListItem>, HttpErrorResponse>(() => ({
      queryKey: [...MY_BATCHES_QUERY_KEY, params()],
      queryFn: () => this._fetchBatches(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  batchDetailQuery(batchId: Signal<string | null>) {
    return injectQuery<MyBatchDetail, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...MY_BATCH_DETAIL_QUERY_KEY, batchId()],
      queryFn: () => this._fetchBatchDetail(batchId() as string),
    }));
  }

  batchSessionsQuery(batchId: Signal<string | null>, params: Signal<MyBatchSessionsQuery>) {
    return injectQuery<PaginatedList<ClassSession>, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...MY_BATCH_SESSIONS_QUERY_KEY, batchId(), params()],
      queryFn: () => this._fetchBatchSessions(batchId() as string, params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  batchAttendanceQuery(batchId: Signal<string | null>, params: Signal<MyBatchAttendanceQuery>) {
    return injectQuery<MyBatchAttendance, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...MY_BATCH_ATTENDANCE_QUERY_KEY, batchId(), params()],
      queryFn: () => this._fetchBatchAttendance(batchId() as string, params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  batchExamResultsQuery(batchId: Signal<string | null>) {
    return injectQuery<MyBatchExamResults, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...MY_BATCH_EXAM_RESULTS_QUERY_KEY, batchId()],
      queryFn: () => this._fetchBatchExamResults(batchId() as string),
      staleTime: 30_000,
    }));
  }

  myBatchMaterialsQuery(batchId: Signal<string | null>, params: Signal<CourseMaterialsQuery>) {
    return injectQuery<PaginatedList<StudyMaterialListItem>, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...MY_BATCH_MATERIALS_QUERY_KEY, batchId(), params()],
      queryFn: () => this._fetchMyBatchMaterials(batchId() as string, params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  async downloadMaterialFile(materialId: string): Promise<{ blob: Blob; fileName: string }> {
    try {
      const response = await firstValueFrom(
        this._http.get(`/me/materials/${materialId}/file`, {
          observe: 'response',
          responseType: 'blob',
        }),
      );
      const fileName =
        parseFileNameFromContentDisposition(response.headers.get('content-disposition')) ??
        'download';
      return { blob: response.body as Blob, fileName };
    } catch {
      throw new Error('Download file error');
    }
  }

  private async _fetchProfile(): Promise<CandidateDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<CandidateDetail>>('/me/profile'),
    );
    if (!response.data) throw new Error(response.message || 'Profile not found');
    return response.data;
  }

  private async _fetchBatches(query: MyBatchListQuery): Promise<PaginatedList<MyBatchListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<MyBatchListItem>>>('/me/batches', { params }),
    );
    return response.data ?? emptyPaginated<MyBatchListItem>(query);
  }

  private async _fetchBatchDetail(batchId: string): Promise<MyBatchDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<MyBatchDetail>>(`/me/batches/${batchId}`),
    );
    if (!response.data) throw new Error(response.message || 'Batch not found');
    return response.data;
  }

  private async _fetchBatchSessions(
    batchId: string,
    query: MyBatchSessionsQuery,
  ): Promise<PaginatedList<ClassSession>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    if (query.fromDate) params = params.set('fromDate', query.fromDate);
    if (query.toDate) params = params.set('toDate', query.toDate);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<ClassSession>>>(`/me/batches/${batchId}/sessions`, {
        params,
      }),
    );
    return response.data ?? emptyPaginated<ClassSession>(query);
  }

  private async _fetchBatchAttendance(
    batchId: string,
    query: MyBatchAttendanceQuery,
  ): Promise<MyBatchAttendance> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    if (query.fromDate) params = params.set('fromDate', query.fromDate);
    if (query.toDate) params = params.set('toDate', query.toDate);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<MyBatchAttendance>>(`/me/batches/${batchId}/attendance`, {
        params,
      }),
    );
    if (!response.data) throw new Error(response.message || 'Attendance not found');
    return response.data;
  }

  private async _fetchBatchExamResults(batchId: string): Promise<MyBatchExamResults> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<MyBatchExamResults>>(`/me/batches/${batchId}/exam-results`),
    );
    if (!response.data) throw new Error(response.message || 'Exam results not found');
    return response.data;
  }

  private async _fetchMyBatchMaterials(
    batchId: string,
    query: CourseMaterialsQuery,
  ): Promise<PaginatedList<StudyMaterialListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<StudyMaterialListItem>>>(
        `/me/batches/${batchId}/materials`,
        { params },
      ),
    );
    return response.data ?? emptyPaginated<StudyMaterialListItem>(query);
  }
}
