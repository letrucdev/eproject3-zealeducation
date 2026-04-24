import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { CourseListItem } from '@core/models/course-list-item';
import { PaginatedList } from '@core/models/paginated-list';

export interface CourseListQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

export interface CreateCoursePayload {
  courseName: string;
  description: string | null;
  durationWeeks: number;
  baseFee: number;
  isActive: boolean;
}

export interface UpdateCoursePayload {
  courseName: string;
  description: string | null;
  durationWeeks: number;
  baseFee: number;
  isActive: boolean;
}

export const COURSE_QUERY_KEY = ['courses'] as const;
export const COURSE_DETAIL_QUERY_KEY = ['course-detail'] as const;

@Injectable({ providedIn: 'root' })
export class CoursesService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<CourseListQuery>) {
    return injectQuery(() => ({
      queryKey: [...COURSE_QUERY_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 60_000,
      placeholderData: keepPreviousData,
    }));
  }

  detailQuery(courseId: Signal<string | null>) {
    return injectQuery<CourseListItem, HttpErrorResponse>(() => ({
      enabled: courseId() !== null,
      queryKey: [...COURSE_DETAIL_QUERY_KEY, courseId()],
      queryFn: () => this._fetchById(courseId() as string),
    }));
  }

  createMutation() {
    return injectMutation<unknown, HttpErrorResponse, CreateCoursePayload>(() => ({
      mutationFn: (payload) =>
        firstValueFrom(this._http.post<ApiResponse<unknown>>('/courses', payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { courseId: string; payload: UpdateCoursePayload }
    >(() => ({
      mutationFn: ({ courseId, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/courses/${courseId}`, payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  searchList(query: CourseListQuery): Promise<PaginatedList<CourseListItem>> {
    return this._queryClient.fetchQuery({
      queryKey: [...COURSE_QUERY_KEY, query],
      queryFn: () => this._fetchList(query),
      staleTime: 30_000,
    });
  }

  private async _fetchList(query: CourseListQuery): Promise<PaginatedList<CourseListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 20));

    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.isActive !== undefined) {
      params = params.set('isActive', String(query.isActive));
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<CourseListItem>>>('/courses', { params }),
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

  private async _fetchById(courseId: string): Promise<CourseListItem> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<CourseListItem>>(`/courses/${courseId}`),
    );
    if (!response.data) throw new Error(response.message || 'Course not found');
    return response.data;
  }

  private _invalidateAll(): void {
    void this._queryClient.invalidateQueries({ queryKey: COURSE_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: COURSE_DETAIL_QUERY_KEY });
  }
}
