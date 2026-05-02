import { FeedbackType } from './feedback-list-item';

export interface FeedbackListQuery {
  page: number;
  pageSize: number;
  search?: string;
  type?: FeedbackType | null;
  batchId?: string | null;
  rating?: number | null;
  isProcessed?: boolean | null;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface SetFeedbackProcessedPayload {
  feedbackId: string;
  isProcessed: boolean;
}
