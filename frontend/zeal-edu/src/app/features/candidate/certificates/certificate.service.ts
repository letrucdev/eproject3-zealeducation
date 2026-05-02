import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, Signal, inject } from '@angular/core';
import { injectMutation, injectQuery } from '@tanstack/angular-query-experimental';
import { QueryClient } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { parseFileNameFromContentDisposition } from '@core/http/content-disposition';
import {
  ApplyForCertificatePayload,
  MyCertificateEligibility,
  MyCertificateListItem,
} from './models/certificate-models';

export const MY_CERTIFICATES_QUERY_KEY = ['my-certificates'] as const;
export const MY_CERTIFICATE_ELIGIBILITY_QUERY_KEY = ['my-certificate-eligibility'] as const;

@Injectable({ providedIn: 'root' })
export class CertificateService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  myCertificatesQuery() {
    return injectQuery<MyCertificateListItem[], HttpErrorResponse>(() => ({
      queryKey: MY_CERTIFICATES_QUERY_KEY,
      queryFn: () => this._fetchMyCertificates(),
      staleTime: 30_000,
    }));
  }

  eligibilityQuery(batchId: Signal<string | null>) {
    return injectQuery<MyCertificateEligibility, HttpErrorResponse>(() => ({
      enabled: batchId() !== null,
      queryKey: [...MY_CERTIFICATE_ELIGIBILITY_QUERY_KEY, batchId()],
      queryFn: () => this._fetchEligibility(batchId() as string),
      staleTime: 30_000,
    }));
  }

  applyMutation() {
    return injectMutation<string, HttpErrorResponse, ApplyForCertificatePayload>(() => ({
      mutationFn: async (payload) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<string>>('/me/certificates/apply', payload),
        );
        if (!response.data) throw new Error(response.message || 'Failed to apply for certificate.');
        return response.data;
      },
      onSuccess: () => {
        void this._queryClient.invalidateQueries({ queryKey: MY_CERTIFICATES_QUERY_KEY });
        void this._queryClient.invalidateQueries({ queryKey: MY_CERTIFICATE_ELIGIBILITY_QUERY_KEY });
      },
    }));
  }

  async downloadCertificateFile(applicationId: string): Promise<{ blob: Blob; fileName: string }> {
    const response = await firstValueFrom(
      this._http.get(`/me/certificates/${applicationId}/file`, {
        observe: 'response',
        responseType: 'blob',
      }),
    );
    const fileName =
      parseFileNameFromContentDisposition(response.headers.get('content-disposition')) ??
      `certificate-${applicationId}.pdf`;
    return { blob: response.body as Blob, fileName };
  }

  private async _fetchMyCertificates(): Promise<MyCertificateListItem[]> {
    const response = await firstValueFrom(
      this._http.get<ApiResponse<MyCertificateListItem[]>>('/me/certificates'),
    );
    return response.data ?? [];
  }

  private async _fetchEligibility(batchId: string): Promise<MyCertificateEligibility> {
    const params = new HttpParams().set('batchId', batchId);
    const response = await firstValueFrom(
      this._http.get<ApiResponse<MyCertificateEligibility>>('/me/certificates/eligibility', {
        params,
      }),
    );
    if (!response.data) throw new Error(response.message || 'Eligibility not available.');
    return response.data;
  }
}
