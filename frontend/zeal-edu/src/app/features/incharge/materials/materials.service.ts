import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { parseFileNameFromContentDisposition } from '@core/http/content-disposition';
import { PaginatedList } from '@core/models/paginated-list';
import { CourseMaterialsQuery, StudyMaterialListItem } from '@core/models/study-material';
import {
  CourseListQuery,
  CreateMaterialsPayload,
  CreateMaterialsResponse,
  MaterialCourseListItem,
  MaterialSiblingCourse,
  ReplaceMaterialFilePayload,
  ToggleMaterialActivePayload,
  UpdateMaterialTitlePayload,
} from './models/material-payload';

export const MATERIAL_COURSES_QUERY_KEY = ['material-courses'] as const;
export const COURSE_MATERIALS_QUERY_KEY = ['course-materials'] as const;

@Injectable({ providedIn: 'root' })
export class MaterialsService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listCoursesQuery(params: Signal<CourseListQuery>) {
    return injectQuery(() => ({
      queryKey: [...MATERIAL_COURSES_QUERY_KEY, params()],
      queryFn: () => this._fetchCourses(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  listCourseMaterialsQuery(
    courseId: Signal<string>,
    params: Signal<CourseMaterialsQuery>,
    enabled: Signal<boolean>,
  ) {
    return injectQuery(() => ({
      queryKey: [...COURSE_MATERIALS_QUERY_KEY, courseId(), params()],
      queryFn: () => this._fetchCourseMaterials(courseId(), params()),
      enabled: enabled(),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  createMutation() {
    return injectMutation<CreateMaterialsResponse, HttpErrorResponse, CreateMaterialsPayload>(
      () => ({
        mutationFn: async (payload) => {
          const fd = new FormData();
          fd.append('title', payload.title);
          for (const courseId of payload.courseIds) {
            fd.append('courseIds', courseId);
          }
          fd.append('file', payload.file, payload.file.name);

          const response = await firstValueFrom(
            this._http.post<ApiResponse<CreateMaterialsResponse>>('/materials', fd),
          );
          if (!response.data) throw new Error(response.message || 'Failed to upload material');
          return response.data;
        },
        onSuccess: () => this._invalidateAll(),
      }),
    );
  }

  updateTitleMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { materialId: string; payload: UpdateMaterialTitlePayload }
    >(() => ({
      mutationFn: ({ materialId, payload }) =>
        firstValueFrom(
          this._http.put<ApiResponse<unknown>>(`/materials/${materialId}/title`, payload),
        ),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  replaceFileMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { materialId: string; payload: ReplaceMaterialFilePayload }
    >(() => ({
      mutationFn: ({ materialId, payload }) => {
        const fd = new FormData();
        fd.append('file', payload.file, payload.file.name);
        return firstValueFrom(
          this._http.put<ApiResponse<unknown>>(`/materials/${materialId}/file`, fd),
        );
      },
      onSuccess: () => this._invalidateAll(),
    }));
  }

  toggleActiveMutation() {
    return injectMutation<
      unknown,
      HttpErrorResponse,
      { materialId: string; payload: ToggleMaterialActivePayload }
    >(() => ({
      mutationFn: ({ materialId, payload }) =>
        firstValueFrom(
          this._http.patch<ApiResponse<unknown>>(`/materials/${materialId}/active`, payload),
        ),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  deleteMutation() {
    return injectMutation<unknown, HttpErrorResponse, string>(() => ({
      mutationFn: (materialId) =>
        firstValueFrom(this._http.delete<ApiResponse<unknown>>(`/materials/${materialId}`)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  async getSiblings(materialId: string): Promise<MaterialSiblingCourse[]> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<MaterialSiblingCourse[]>>(`/materials/${materialId}/siblings`),
    );
    return response.data ?? [];
  }

  async downloadFile(materialId: string): Promise<{ blob: Blob; fileName: string }> {
    try {
      const response = await firstValueFrom(
        this._http.get(`/materials/${materialId}/file`, {
          observe: 'response',
          responseType: 'blob',
        }),
      );
      const fileName =
        parseFileNameFromContentDisposition(response.headers.get('content-disposition')) ??
        'download';
      return { blob: response.body as Blob, fileName };
    } catch (err) {
      throw new Error('Download file error');
    }
  }

  private async _fetchCourses(query: CourseListQuery): Promise<PaginatedList<MaterialCourseListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<MaterialCourseListItem>>>('/materials/courses', {
        params,
      }),
    );
    return response.data ?? emptyPage<MaterialCourseListItem>(query.page);
  }

  private async _fetchCourseMaterials(
    courseId: string,
    query: CourseMaterialsQuery,
  ): Promise<PaginatedList<StudyMaterialListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page))
      .set('pageSize', String(query.pageSize));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.includeInactive !== undefined) {
      params = params.set('includeInactive', String(query.includeInactive));
    }

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<StudyMaterialListItem>>>(
        `/materials/courses/${courseId}/items`,
        { params },
      ),
    );
    return response.data ?? emptyPage<StudyMaterialListItem>(query.page);
  }

  private _invalidateAll(): void {
    void this._queryClient.invalidateQueries({ queryKey: MATERIAL_COURSES_QUERY_KEY });
    void this._queryClient.invalidateQueries({ queryKey: COURSE_MATERIALS_QUERY_KEY });
  }
}

function emptyPage<T>(pageNumber: number): PaginatedList<T> {
  return {
    items: [],
    pageNumber,
    totalPages: 0,
    totalCount: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}
