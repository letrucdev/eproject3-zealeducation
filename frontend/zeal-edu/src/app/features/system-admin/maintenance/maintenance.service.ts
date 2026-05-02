import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { QueryClient } from '@tanstack/query-core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '../../../core/http/api-response';
import { SystemAssetDetail } from '../../../core/models/system-asset-detail';
import { SystemAssetListItem } from '../../../core/models/system-asset-list-item';
import {
  CreateAssetPayload,
  UpdateAssetPayload,
  UpdateAssetConditionPayload,
} from './models/asset-payload';

export const ASSET_QUERY_KEY        = ['system-assets'] as const;
export const ASSET_DETAIL_QUERY_KEY = ['system-asset-detail'] as const;

@Injectable({ providedIn: 'root' })
export class MaintenanceService {
  private readonly _http        = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery() {
    return injectQuery(() => ({
      queryKey: ASSET_QUERY_KEY,
      queryFn:  () => this._fetchList(),
    }));
  }

  detailQuery(id: Signal<string | null>) {
    return injectQuery<SystemAssetDetail, HttpErrorResponse>(() => ({
      enabled:  id() !== null,
      queryKey: [...ASSET_DETAIL_QUERY_KEY, id()],
      queryFn:  () => this._fetchDetail(id() as string),
    }));
  }

  createMutation() {
    return injectMutation<unknown, HttpErrorResponse, CreateAssetPayload>(() => ({
      mutationFn: (payload) =>
        firstValueFrom(this._http.post<ApiResponse<unknown>>('/system-assets', payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateMutation() {
    return injectMutation<
      unknown, HttpErrorResponse,
      { id: string; payload: UpdateAssetPayload }
    >(() => ({
      mutationFn: ({ id, payload }) =>
        firstValueFrom(this._http.put<ApiResponse<unknown>>(`/system-assets/${id}`, payload)),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  updateConditionMutation() {
    return injectMutation<
      unknown, HttpErrorResponse,
      { id: string; payload: UpdateAssetConditionPayload }
    >(() => ({
      mutationFn: ({ id, payload }) =>
        firstValueFrom(
          this._http.patch<ApiResponse<unknown>>(`/system-assets/${id}/condition`, payload),
        ),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  /**
   * Hard delete: removes the asset permanently via DELETE endpoint.
   */
  deleteMutation() {
    return injectMutation<unknown, HttpErrorResponse, string>(() => ({
      mutationFn: (id) =>
        firstValueFrom(
          this._http.delete<ApiResponse<unknown>>(`/system-assets/${id}`),
        ),
      onSuccess: () => this._invalidateAll(),
    }));
  }

  private async _fetchList(): Promise<SystemAssetListItem[]> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<SystemAssetListItem[]>>('/system-assets'),
    );
    return response.data ?? [];
  }

  private async _fetchDetail(id: string): Promise<SystemAssetDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<SystemAssetDetail>>(`/system-assets/${id}`),
    );
    if (!response.data) throw new Error(response.message || 'Asset not found');
    return response.data;
  }

  private _invalidateAll(): void {
    void this._queryClient.refetchQueries({ queryKey: ASSET_QUERY_KEY });
    this._queryClient.removeQueries({ queryKey: ASSET_DETAIL_QUERY_KEY });
  }
}
