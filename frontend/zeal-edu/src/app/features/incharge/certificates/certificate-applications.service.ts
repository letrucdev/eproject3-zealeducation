import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { PaginatedList } from '@core/models/paginated-list';
import {
  CertificateApplicationListItem,
  CertificateApplicationListQuery,
} from './models/certificate-application';

export const CERTIFICATE_APPLICATIONS_QUERY_KEY = ['incharge-certificate-applications'] as const;

interface ApproveResponse {
  applicationId: string;
  certificateNumber: string;
}

interface RegenerateResponse {
  applicationId: string;
  certificateNumber: string;
}

@Injectable({ providedIn: 'root' })
export class CertificateApplicationsService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<CertificateApplicationListQuery>) {
    return injectQuery(() => ({
      queryKey: [...CERTIFICATE_APPLICATIONS_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  approveMutation() {
    return injectMutation<ApproveResponse, HttpErrorResponse, { applicationId: string }>(() => ({
      mutationFn: async ({ applicationId }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<ApproveResponse>>(
            `/incharge/certificates/${applicationId}/approve`,
            {},
          ),
        );
        if (!response.data) throw new Error(response.message || 'Failed to approve.');
        return response.data;
      },
      onSuccess: () => {
        void this._queryClient.invalidateQueries({ queryKey: CERTIFICATE_APPLICATIONS_QUERY_KEY });
      },
    }));
  }

  regenerateMutation() {
    return injectMutation<RegenerateResponse, HttpErrorResponse, { applicationId: string }>(() => ({
      mutationFn: async ({ applicationId }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<RegenerateResponse>>(
            `/incharge/certificates/${applicationId}/regenerate`,
            {},
          ),
        );
        if (!response.data) throw new Error(response.message || 'Failed to regenerate.');
        return response.data;
      },
      onSuccess: () => {
        void this._queryClient.invalidateQueries({ queryKey: CERTIFICATE_APPLICATIONS_QUERY_KEY });
      },
    }));
  }

  private async _fetchList(
    query: CertificateApplicationListQuery,
  ): Promise<PaginatedList<CertificateApplicationListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.status) params = params.set('status', query.status);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<CertificateApplicationListItem>>>(
        '/incharge/certificates',
        { params },
      ),
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
}
