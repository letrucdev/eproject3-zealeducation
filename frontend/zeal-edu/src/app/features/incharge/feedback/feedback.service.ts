import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { PaginatedList } from '@core/models/paginated-list';
import { FeedbackListItem } from './models/feedback-list-item';
import { FeedbackListQuery, SetFeedbackProcessedPayload } from './models/feedback-payload';

export const FEEDBACK_QUERY_KEY = ['incharge-feedback'] as const;

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<FeedbackListQuery>) {
    return injectQuery(() => ({
      queryKey: [...FEEDBACK_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  setProcessedMutation() {
    return injectMutation<unknown, HttpErrorResponse, SetFeedbackProcessedPayload>(() => ({
      mutationFn: ({ feedbackId, isProcessed }) =>
        firstValueFrom(
          this._http.patch<ApiResponse<unknown>>(
            `/incharge/feedback/${feedbackId}/processed`,
            { isProcessed },
          ),
        ),
      onSuccess: () => this._invalidateList(),
    }));
  }

  private async _fetchList(query: FeedbackListQuery): Promise<PaginatedList<FeedbackListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.type) params = params.set('type', query.type);
    if (query.batchId) params = params.set('batchId', query.batchId);
    if (query.rating != null) params = params.set('rating', String(query.rating));
    if (query.isProcessed != null) params = params.set('isProcessed', String(query.isProcessed));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FeedbackListItem>>>('/incharge/feedback', { params }),
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

  private _invalidateList(): void {
    void this._queryClient.invalidateQueries({ queryKey: FEEDBACK_QUERY_KEY });
  }
}
