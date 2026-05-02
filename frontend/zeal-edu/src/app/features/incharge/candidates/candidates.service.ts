import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { PaginatedList } from '@core/models/paginated-list';
import {
  PAYMENTS_DETAIL_KEY,
  PAYMENTS_LIST_KEY,
} from '@core/services/payments.service';
import { CandidateDetail } from '@core/models/candidate-detail';
import { CandidateListItem } from './models/candidate-list-item';
import {
  ApplyFinePayload,
  ApplyFineResponse,
  CandidateListQuery,
  ResetCandidatePasswordResponse,
  UpdateCandidatePayload,
} from './models/candidate-payload';
import {
  CandidateRegistrationTrendPoint,
  RegistrationsTrendRange,
} from './models/candidate-registration-trend';

export const CANDIDATE_QUERY_KEY = ['candidates'] as const;
export const CANDIDATE_DETAIL_QUERY_KEY = ['candidate-detail'] as const;
export const CANDIDATE_TREND_QUERY_KEY = ['candidate-registrations-trend'] as const;

@Injectable({ providedIn: 'root' })
export class CandidatesService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<CandidateListQuery>) {
    return injectQuery(() => ({
      queryKey: [...CANDIDATE_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  registrationsTrendQuery(days: Signal<RegistrationsTrendRange>) {
    return injectQuery(() => ({
      queryKey: [...CANDIDATE_TREND_QUERY_KEY, days()],
      queryFn: () => this._fetchTrend(days()),
      staleTime: 60_000,
    }));
  }

  detailQuery(candidateId: Signal<string | null>) {
    return injectQuery<CandidateDetail, HttpErrorResponse>(() => ({
      enabled: candidateId() !== null,
      queryKey: [...CANDIDATE_DETAIL_QUERY_KEY, candidateId()],
      queryFn: () => this._fetchDetail(candidateId() as string),
    }));
  }

  updateMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { candidateId: string; payload: UpdateCandidatePayload }
    >(() => ({
      mutationFn: ({ candidateId, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/candidates/${candidateId}`, payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  resetPasswordMutation() {
    return injectMutation<ResetCandidatePasswordResponse, HttpErrorResponse, { candidateId: string }>(
      () => ({
        mutationFn: async ({ candidateId }) => {
          const response = await firstValueFrom(
            this._http.post<ApiResponse<ResetCandidatePasswordResponse>>(
              `/candidates/${candidateId}/reset-password`,
              {},
            ),
          );
          if (!response.data) throw new Error(response.message || 'Failed to reset password');
          return response.data;
        },
      }),
    );
  }

  applyFineMutation() {
    return injectMutation<
      ApplyFineResponse,
      HttpErrorResponse,
      { candidateId: string; payload: ApplyFinePayload }
    >(() => ({
      mutationFn: async ({ candidateId, payload }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<ApplyFineResponse>>(
            `/candidates/${candidateId}/fines`,
            payload,
          ),
        );
        if (!response.data) throw new Error(response.message || 'Failed to apply fine');
        return response.data;
      },
      onSuccess: () => {
        this._invalidateAll();
      },
    }));
  }

  private async _fetchList(query: CandidateListQuery): Promise<PaginatedList<CandidateListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.status) params = params.set('status', query.status);
    if (query.courseId) params = params.set('courseId', query.courseId);
    if (query.batchId) params = params.set('batchId', query.batchId);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<CandidateListItem>>>('/candidates', { params }),
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

  private async _fetchTrend(
    days: RegistrationsTrendRange,
  ): Promise<CandidateRegistrationTrendPoint[]> {
    const params = new HttpParams().set('days', String(days));
    const response = await firstValueFrom(
      this._http.get<ApiResponse<CandidateRegistrationTrendPoint[]>>(
        '/incharge/candidates/registrations-trend',
        { params },
      ),
    );
    return response.data ?? [];
  }

  private async _fetchDetail(candidateId: string): Promise<CandidateDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<CandidateDetail>>(`/candidates/${candidateId}`),
    );
    if (!response.data) throw new Error(response.message || 'Candidate not found');
    return response.data;
  }

  private _invalidateAll(): void {
    void this._queryClient.invalidateQueries({ queryKey: CANDIDATE_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: PAYMENTS_LIST_KEY });
    void this._queryClient.invalidateQueries({ queryKey: PAYMENTS_DETAIL_KEY });
    void this._queryClient.invalidateQueries({ queryKey: CANDIDATE_DETAIL_QUERY_KEY });
  }
}
