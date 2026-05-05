import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient, keepPreviousData } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { PaginatedList } from '@core/models/paginated-list';
import { FeeStructureDetail, FeeStructureListItem } from '@core/models/fee-structure';
import {
  FinancialReport,
  FinancialReportExportParams,
  FinancialReportRange,
  FinancialTransactionListItem,
  FinancialTransactionsQuery,
} from '@core/models/financial-report';
import {
  ConfirmPaymentPayload,
  ConfirmPaymentResponse,
  FeeStructureListQuery,
  SetPaymentTypePayload,
  SetPaymentTypeResponse,
} from '@core/models/payment-payload';

export const PAYMENTS_LIST_KEY = ['payments', 'fee-structures'] as const;
export const PAYMENTS_DETAIL_KEY = ['payments', 'fee-structure-detail'] as const;
export const FINANCIAL_REPORT_KEY = ['payments', 'financial-report'] as const;
export const FINANCIAL_TRANSACTIONS_KEY = ['payments', 'financial-transactions'] as const;

@Injectable({ providedIn: 'root' })
export class PaymentsService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  listQuery(params: Signal<FeeStructureListQuery>) {
    return injectQuery(() => ({
      queryKey: [...PAYMENTS_LIST_KEY, params()],
      queryFn: () => this._fetchList(params()),
      refetchInterval: 30_000,
      refetchIntervalInBackground: true,
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

  financialReportQuery(params: Signal<FinancialReportRange>) {
    return injectQuery(() => ({
      queryKey: [...FINANCIAL_REPORT_KEY, params()],
      queryFn: () => this._fetchFinancialReport(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  financialTransactionsQuery(params: Signal<FinancialTransactionsQuery>) {
    return injectQuery(() => ({
      queryKey: [...FINANCIAL_TRANSACTIONS_KEY, params()],
      queryFn: () => this._fetchFinancialTransactions(params()),
      staleTime: 30_000,
      placeholderData: keepPreviousData,
    }));
  }

  async downloadFinancialReportExcel(params: FinancialReportExportParams): Promise<void> {
    let httpParams = new HttpParams()
      .set('trendFrom', params.revenueTrend.from)
      .set('trendTo', params.revenueTrend.to)
      .set('statusFrom', params.paymentStatus.from)
      .set('statusTo', params.paymentStatus.to)
      .set('feeTypeFrom', params.revenueByFeeType.from)
      .set('feeTypeTo', params.revenueByFeeType.to)
      .set('topCoursesFrom', params.topCourses.from)
      .set('topCoursesTo', params.topCourses.to)
      .set('txFrom', params.transactions.from)
      .set('txTo', params.transactions.to);
    if (params.search && params.search.trim().length > 0) {
      httpParams = httpParams.set('search', params.search.trim());
    }
    if (params.feeType) httpParams = httpParams.set('feeType', params.feeType);
    if (params.method) httpParams = httpParams.set('method', params.method);

    const response = await firstValueFrom(
      this._http.get('/payments/financial-report/export', {
        params: httpParams,
        responseType: 'blob',
        observe: 'response',
      }),
    );
    const blob = response.body as Blob;
    const fileName =
      this._extractFileName(response.headers.get('Content-Disposition')) ??
      `financial-report-${params.transactions.from}-${params.transactions.to}.xlsx`;

    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(url), 1000);
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
    const contentType =
      response.headers.get('Content-Type') ?? blob.type ?? 'application/octet-stream';
    return { objectUrl: URL.createObjectURL(blob), contentType };
  }

  private async _fetchList(
    query: FeeStructureListQuery,
  ): Promise<PaginatedList<FeeStructureListItem>> {
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
    void this._queryClient.invalidateQueries({ queryKey: FINANCIAL_REPORT_KEY });
    void this._queryClient.invalidateQueries({ queryKey: FINANCIAL_TRANSACTIONS_KEY });
  }

  private async _fetchFinancialReport(range: FinancialReportRange): Promise<FinancialReport> {
    const params = new HttpParams().set('from', range.from).set('to', range.to);
    const response = await firstValueFrom(
      this._http.get<ApiResponse<FinancialReport>>('/payments/financial-report', { params }),
    );
    if (!response.data) throw new Error(response.message || 'Empty financial report');
    return response.data;
  }

  private async _fetchFinancialTransactions(
    query: FinancialTransactionsQuery,
  ): Promise<PaginatedList<FinancialTransactionListItem>> {
    let params = new HttpParams()
      .set('from', query.from)
      .set('to', query.to)
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));
    if (query.search && query.search.trim().length > 0) {
      params = params.set('search', query.search.trim());
    }
    if (query.feeType) params = params.set('feeType', query.feeType);
    if (query.method) params = params.set('method', query.method);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    const response = await firstValueFrom(
      this._http.get<ApiResponse<PaginatedList<FinancialTransactionListItem>>>(
        '/payments/financial-report/transactions',
        { params },
      ),
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

  private _extractFileName(contentDisposition: string | null): string | null {
    if (!contentDisposition) return null;
    const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(contentDisposition);
    return match ? decodeURIComponent(match[1]) : null;
  }
}
