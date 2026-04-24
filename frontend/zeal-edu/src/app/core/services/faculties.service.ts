import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { FacultyListItem } from '@core/models/faculty-list-item';
import { PaginatedList } from '@core/models/paginated-list';

export interface FacultyListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
}

export const FACULTY_QUERY_KEY = ['faculties'] as const;

@Injectable({ providedIn: 'root' })
export class FacultiesService {
  private readonly _http = inject(HttpClient);

  listQuery(params: Signal<FacultyListQuery>) {
    return injectQuery(() => ({
      queryKey: [...FACULTY_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 60_000,
      placeholderData: keepPreviousData,
    }));
  }

  private async _fetchList(query: FacultyListQuery): Promise<PaginatedList<FacultyListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 20));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FacultyListItem>>>('/faculty', { params }),
    );
    return (
      response.data ?? {
        items: [],
        pageNumber: query.page ?? 1,
        totalPages: 0,
        totalCount: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      }
    );
  }
}
