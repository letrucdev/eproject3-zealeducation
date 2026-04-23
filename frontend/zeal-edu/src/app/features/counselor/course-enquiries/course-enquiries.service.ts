import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { QueryClient } from '@tanstack/query-core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '../../../core/http/api-response';
import { ConvertEnquiryResult } from '../../../core/models/convert-enquiry-result';
import { CourseEnquiryDetail } from '../../../core/models/course-enquiry-detail';
import { CourseEnquiryListItem } from '../../../core/models/course-enquiry-list-item';
import { EnquiryNote } from '../../../core/models/enquiry-note';
import { EnquiryStatistics } from '../../../core/models/enquiry-statistics';
import { PaginatedList } from '../../../core/models/paginated-list';
import {
  AddEnquiryNotePayload,
  ConvertEnquiryPayload,
  CreateEnquiryPayload,
  EnquiryListQuery,
  UpdateEnquiryPayload,
} from './models/course-enquiry-payload';

export const ENQUIRY_QUERY_KEY = ['course-enquiries'] as const;
export const ENQUIRY_STATS_QUERY_KEY = ['course-enquiry-statistics'] as const;
export const ENQUIRY_DETAIL_QUERY_KEY = ['course-enquiry-detail'] as const;

@Injectable({ providedIn: 'root' })
export class CourseEnquiriesService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<EnquiryListQuery>) {
    return injectQuery(() => ({
      queryKey: [...ENQUIRY_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
    }));
  }

  statisticsQuery() {
    return injectQuery(() => ({
      queryKey: ENQUIRY_STATS_QUERY_KEY,
      queryFn: () => this._fetchStatistics(),
    }));
  }

  detailQuery(enquiryId: Signal<string | null>) {
    return injectQuery<CourseEnquiryDetail, HttpErrorResponse>(() => ({
      enabled: enquiryId() !== null,
      queryKey: [...ENQUIRY_DETAIL_QUERY_KEY, enquiryId()],
      queryFn: () => this._fetchDetail(enquiryId() as string),
    }));
  }

  createMutation() {
    return injectMutation<unknown, HttpErrorResponse, CreateEnquiryPayload>(() => ({
      mutationFn: (payload) =>
        firstValueFrom(this._http.post<ApiResponse<unknown>>('/course-enquiries', payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { enquiryId: string; payload: UpdateEnquiryPayload }
    >(() => ({
      mutationFn: ({ enquiryId, payload }) =>
        firstValueFrom(
          this._http.put<ApiResponse<unknown>>(`/course-enquiries/${enquiryId}`, payload),
        ),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  addNoteMutation() {
    return injectMutation<
      EnquiryNote,
      HttpErrorResponse,
      { enquiryId: string; payload: AddEnquiryNotePayload }
    >(() => ({
      mutationFn: async ({ enquiryId, payload }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<EnquiryNote>>(
            `/course-enquiries/${enquiryId}/notes`,
            payload,
          ),
        );
        if (!response.data) throw new Error(response.message || 'Failed to add note');
        return response.data;
      },
      onSuccess: () => this._invalidateAll(),
    }));
  }

  convertMutation() {
    return injectMutation<
      ConvertEnquiryResult,
      HttpErrorResponse,
      { enquiryId: string; payload: ConvertEnquiryPayload }
    >(() => ({
      mutationFn: async ({ enquiryId, payload }) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<ConvertEnquiryResult>>(
            `/course-enquiries/${enquiryId}/convert`,
            payload,
          ),
        );
        if (!response.data) throw new Error(response.message || 'Failed to convert');
        return response.data;
      },
      onSuccess: () => this._invalidateAll(),
    }));
  }

  private async _fetchList(query: EnquiryListQuery): Promise<PaginatedList<CourseEnquiryListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.status) params = params.set('status', query.status);
    if (query.source) params = params.set('source', query.source);
    if (query.dueFollowUpOnly) params = params.set('dueFollowUpOnly', 'true');

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<CourseEnquiryListItem>>>('/course-enquiries', {
        params,
      }),
    );
    return response.data ?? this._emptyPage(query);
  }

  private async _fetchStatistics(): Promise<EnquiryStatistics> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<EnquiryStatistics>>('/course-enquiries/statistics'),
    );
    return response.data ?? { total: 0, new: 0, inFollowUp: 0, converted: 0, overdue: 0 };
  }

  private async _fetchDetail(enquiryId: string): Promise<CourseEnquiryDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<CourseEnquiryDetail>>(`/course-enquiries/${enquiryId}`),
    );
    if (!response.data) throw new Error(response.message || 'Enquiry not found');
    return response.data;
  }

  private _emptyPage(query: EnquiryListQuery): PaginatedList<CourseEnquiryListItem> {
    return {
      items: [],
      pageNumber: query.page,
      totalPages: 0,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }

  private _invalidateAll(): void {
    void this._queryClient.invalidateQueries({ queryKey: ENQUIRY_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: ENQUIRY_DETAIL_QUERY_KEY });
    void this._queryClient.refetchQueries({ queryKey: ENQUIRY_STATS_QUERY_KEY });
  }
}
