import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { QueryClient } from '@tanstack/query-core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { PaginatedList } from '@core/models/paginated-list';
import { StaffDetail } from '@core/models/staff-detail';
import { StaffListItem } from '@core/models/staff-list-item';
import { StaffStatistics } from '@core/models/staff-statistics';
import {
  CreateFacultyPayload,
  CreateStaffPayload,
  StaffListQuery,
  UpdateFacultyPayload,
  UpdateStaffPayload,
} from './models/staff-form-payload';

export const STAFF_QUERY_KEY = ['staffs'] as const;
export const STAFF_STATS_QUERY_KEY = ['staff-statistics'] as const;
export const STAFF_DETAIL_QUERY_KEY = ['staff-detail'] as const;

@Injectable({ providedIn: 'root' })
export class StaffAccountsService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<StaffListQuery>) {
    return injectQuery(() => ({
      queryKey: [...STAFF_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
    }));
  }

  statisticsQuery() {
    return injectQuery(() => ({
      queryKey: STAFF_STATS_QUERY_KEY,
      queryFn: () => this._fetchStatistics(),
    }));
  }

  detailQuery(staffId: Signal<string | null>) {
    return injectQuery<StaffDetail, HttpErrorResponse>(() => ({
      enabled: staffId() !== null,
      queryKey: [...STAFF_DETAIL_QUERY_KEY, staffId()],
      queryFn: () => this._fetchDetail(staffId() as string),
    }));
  }

  createMutation() {
    return injectMutation<unknown, HttpErrorResponse, CreateStaffPayload>(() => ({
      mutationFn: (payload) =>
        firstValueFrom(this._http.post<ApiResponse<unknown>>('/staff', payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { staffId: string; payload: UpdateStaffPayload }
    >(() => ({
      mutationFn: ({ staffId, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/staff/${staffId}`, payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  createFacultyMutation() {
    return injectMutation<unknown, HttpErrorResponse, CreateFacultyPayload>(() => ({
      mutationFn: (payload) =>
        firstValueFrom(this._http.post<ApiResponse<unknown>>('/faculty', payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateFacultyMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { staffId: string; payload: UpdateFacultyPayload }
    >(() => ({
      mutationFn: ({ staffId, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/faculty/${staffId}`, payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  private async _fetchList(query: StaffListQuery): Promise<PaginatedList<StaffListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.role) {
      params = params.set('role', query.role);
    }
    if (query.isActive !== undefined) {
      params = params.set('isActive', String(query.isActive));
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<StaffListItem>>>('/staff', { params }),
    );
    return response.data ?? this._emptyPage(query);
  }

  private async _fetchStatistics(): Promise<StaffStatistics> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<StaffStatistics>>('/staff/statistics'),
    );
    return (
      response.data ?? {
        total: 0,
        active: 0,
        inactive: 0,
        incharge: 0,
        counselor: 0,
        accountsStaff: 0,
      }
    );
  }

  private async _fetchDetail(staffId: string): Promise<StaffDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<StaffDetail>>(`/staff/${staffId}`),
    );
    if (!response.data) {
      throw new Error(response.message || 'Staff not found');
    }
    return response.data;
  }

  private _emptyPage(query: StaffListQuery): PaginatedList<StaffListItem> {
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
    void this._queryClient.refetchQueries({ queryKey: STAFF_QUERY_KEY });
    void this._queryClient.refetchQueries({ queryKey: STAFF_STATS_QUERY_KEY });
    this._queryClient.removeQueries({ queryKey: STAFF_DETAIL_QUERY_KEY });
  }
}
