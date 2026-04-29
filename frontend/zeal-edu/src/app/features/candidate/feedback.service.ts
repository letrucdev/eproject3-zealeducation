import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { injectMutation } from '@tanstack/angular-query-experimental';
import { QueryClient } from '@tanstack/query-core';
import { firstValueFrom } from 'rxjs';
import { ApiResponse } from '@core/http/api-response';
import { MY_BATCH_DETAIL_QUERY_KEY } from './candidate-portal.service';
import {
  SubmitCourseFeedbackPayload,
  SubmitFacultyFeedbackPayload,
  SubmitGeneralFeedbackPayload,
} from './models/feedback-payload';

@Injectable({ providedIn: 'root' })
export class FeedbackService {
  private readonly _http = inject(HttpClient);
  private readonly _queryClient = inject(QueryClient);

  submitFacultyMutation() {
    return injectMutation<string, HttpErrorResponse, SubmitFacultyFeedbackPayload>(() => ({
      mutationFn: async (payload) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<string>>('/me/feedback/faculty', payload),
        );
        if (!response.data) throw new Error(response.message || 'Failed to submit feedback');
        return response.data;
      },
      onSuccess: () => this._invalidateBatchDetail(),
    }));
  }

  submitCourseMutation() {
    return injectMutation<string, HttpErrorResponse, SubmitCourseFeedbackPayload>(() => ({
      mutationFn: async (payload) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<string>>('/me/feedback/course', payload),
        );
        if (!response.data) throw new Error(response.message || 'Failed to submit feedback');
        return response.data;
      },
      onSuccess: () => this._invalidateBatchDetail(),
    }));
  }

  submitGeneralMutation() {
    return injectMutation<string, HttpErrorResponse, SubmitGeneralFeedbackPayload>(() => ({
      mutationFn: async (payload) => {
        const response = await firstValueFrom(
          this._http.post<ApiResponse<string>>('/me/feedback/general', payload),
        );
        if (!response.data) throw new Error(response.message || 'Failed to submit feedback');
        return response.data;
      },
      onSuccess: () => this._invalidateBatchDetail(),
    }));
  }

  private _invalidateBatchDetail(): void {
    void this._queryClient.invalidateQueries({ queryKey: MY_BATCH_DETAIL_QUERY_KEY });
  }
}
