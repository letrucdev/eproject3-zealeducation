import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { BatchDetail } from '@core/models/batch-detail';
import { BatchListItem } from '@core/models/batch-list-item';
import { BatchStatistics } from '@core/models/batch-statistics';
import { PaginatedList } from '@core/models/paginated-list';
import {
  AssignFacultyPayload,
  BatchListQuery,
  CreateBatchPayload,
  UpdateBatchPayload,
} from './models/batch-payload';

export const BATCH_QUERY_KEY = ['batches'] as const;
export const BATCH_DETAIL_QUERY_KEY = ['batch-detail'] as const;
export const BATCH_STATS_QUERY_KEY = ['batch-statistics'] as const;

interface CreateBatchResponse {
  batchId: string;
  batchCode: string;
  status: string;
}

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
    return (
      response.data ?? {
        items: [],
        pageNumber: query.page,
        totalPages: 0,
        totalCount: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      }
    );
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

  private _invalidateAll(): void {
    void this._queryClient.invalidateQueries({ queryKey: BATCH_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: BATCH_STATS_QUERY_KEY });
    void this._queryClient.removeQueries({ queryKey: BATCH_DETAIL_QUERY_KEY });
  }
}
