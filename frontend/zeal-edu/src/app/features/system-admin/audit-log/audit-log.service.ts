import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { AuditLogDetail } from '@core/models/audit-log-detail';
import { AuditLogListItem } from '@core/models/audit-log-list-item';
import { PaginatedList } from '@core/models/paginated-list';
import { AuditLogListQuery } from './models/audit-log-list-query';

export const AUDIT_LOG_QUERY_KEY = ['audit-logs'] as const;
export const AUDIT_LOG_DETAIL_QUERY_KEY = ['audit-log-detail'] as const;

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly _http = inject(HttpClient);

  listQuery(params: Signal<AuditLogListQuery>) {
    return injectQuery(() => ({
      queryKey: [...AUDIT_LOG_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
    }));
  }

  detailQuery(id: Signal<string | null>) {
    return injectQuery<AuditLogDetail, HttpErrorResponse>(() => ({
      enabled: id() !== null,
      queryKey: [...AUDIT_LOG_DETAIL_QUERY_KEY, id()],
      queryFn: () => this._fetchDetail(id() as string),
    }));
  }

  private async _fetchList(query: AuditLogListQuery): Promise<PaginatedList<AuditLogListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.action) {
      params = params.set('action', query.action);
    }
    if (query.userId) {
      params = params.set('userId', query.userId);
    }
    if (query.tableName && query.tableName.trim().length > 0) {
      params = params.set('tableName', query.tableName.trim());
    }
    if (query.fromDate) {
      params = params.set('fromDate', query.fromDate);
    }
    if (query.toDate) {
      params = params.set('toDate', query.toDate);
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<AuditLogListItem>>>('/audit-logs', { params }),
    );
    return response.data ?? this._emptyPage(query);
  }

  private async _fetchDetail(id: string): Promise<AuditLogDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<AuditLogDetail>>(`/audit-logs/${id}`),
    );
    if (!response.data) {
      throw new Error(response.message || 'Audit log entry not found');
    }
    return response.data;
  }

  private _emptyPage(query: AuditLogListQuery): PaginatedList<AuditLogListItem> {
    return {
      items: [],
      pageNumber: query.page,
      totalPages: 0,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  }
}
