import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { PaginatedList } from '@core/models/paginated-list';
import { FeeStructureDetail, FeeStructureListItem } from '@core/models/fee-structure';
import {
  ConfirmPaymentPayload,
  ConfirmPaymentResponse,
  FeeStructureListQuery,
  SetPaymentTypePayload,
  SetPaymentTypeResponse,
} from '@core/models/payment-payload';

export const PAYMENTS_LIST_KEY = ['payments', 'fee-structures'] as const;
export const PAYMENTS_DETAIL_KEY = ['payments', 'fee-structure-detail'] as const;

@Injectable({ providedIn: 'root' })
export class PaymentsService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<FeeStructureListQuery>) {
    return injectQuery(() => ({
      queryKey: [...PAYMENTS_LIST_KEY, params()],
      queryFn: () => this._fetchList(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  detailQuery(feeId: Signal<string | null>) {
    return injectQuery<FeeStructureDetail, HttpErrorResponse>(() => ({
      enabled: feeId() !== null,
      queryKey: [...PAYMENTS_DETAIL_KEY, feeId()],
      queryFn: () => this._fetchDetail(feeId() as string),
    }));
  }

  setPaymentTypeMutation() {
    return injectMutation<
      SetPaymentTypeResponse,
      HttpErrorResponse,
      { feeId: string; payload: SetPaymentTypePayload }
    >(() => ({
      mutationFn: async ({ feeId, payload }) => {
        const response = await firstValueFrom(
          this._http.put<ApiResponse<SetPaymentTypeResponse>>(
            `/payments/fee-structures/${feeId}/payment-type`,
            payload,
          ),
        );
        if (!response.data) throw new Error(response.message || 'Empty response');
        return response.data;
      },
      onSuccess: () => this._invalidate(),
    }));
  }

  confirmPaymentMutation() {
    return injectMutation<
      ConfirmPaymentResponse,
      HttpErrorResponse,
      { feeId: string; payload: ConfirmPaymentPayload }
    >(() => ({
      mutationFn: async ({ feeId, payload }) => {
        const formData = new FormData();
        formData.append('paymentMethod', payload.paymentMethod);
        if (payload.installmentPlanId) {
          formData.append('installmentPlanId', payload.installmentPlanId);
        }
        if (payload.proofFile) {
          formData.append('bankTransferProof', payload.proofFile, payload.proofFile.name);
        }
        const response = await firstValueFrom(
          this._http.post<ApiResponse<ConfirmPaymentResponse>>(
            `/payments/fee-structures/${feeId}/confirm`,
            formData,
          ),
        );
        if (!response.data) throw new Error(response.message || 'Empty response');
        return response.data;
      },
      onSuccess: () => this._invalidate(),
    }));
  }

  async downloadReceipt(transactionId: string, fileName?: string): Promise<void> {
    const blob = await firstValueFrom(
      this._http.get(`/payments/transactions/${transactionId}/receipt`, {
        responseType: 'blob',
      }),
    );
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName ?? `receipt-${transactionId}.pdf`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  async getBankTransferProofObjectUrl(
    transactionId: string,
  ): Promise<{ objectUrl: string; contentType: string }> {
    const response = await firstValueFrom(
      this._http.get(`/payments/transactions/${transactionId}/bank-transfer-proof`, {
        responseType: 'blob',
        observe: 'response',
      }),
    );
    const blob = response.body as Blob;
    const contentType = response.headers.get('Content-Type') ?? blob.type ?? 'application/octet-stream';
    return { objectUrl: URL.createObjectURL(blob), contentType };
  }

  private async _fetchList(query: FeeStructureListQuery): Promise<PaginatedList<FeeStructureListItem>> {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.status) params = params.set('status', query.status);
    if (query.type) params = params.set('type', query.type);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FeeStructureListItem>>>('/payments/fee-structures', {
        params,
      }),
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

  private async _fetchDetail(feeId: string): Promise<FeeStructureDetail> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<FeeStructureDetail>>(`/payments/fee-structures/${feeId}`),
    );
    if (!response.data) throw new Error(response.message || 'Fee structure not found');
    return response.data;
  }

  private _invalidate(): void {
    void this._queryClient.invalidateQueries({ queryKey: PAYMENTS_LIST_KEY });
    void this._queryClient.invalidateQueries({ queryKey: PAYMENTS_DETAIL_KEY });
  }
}
