export type FeedbackType = 'Faculty' | 'Course' | 'General';

export interface FeedbackListItem {
  feedbackId: string;
  candidateId: string;
  candidateCode: string;
  candidateName: string;
  batchId: string;
  batchCode: string;
  courseName: string;
  type: FeedbackType;
  targetFacultyId: string | null;
  targetFacultyName: string | null;
  rating: number;
  comment: string | null;
  isProcessed: boolean;
  processedById: string | null;
  processedByName: string | null;
  processedAt: string | null;
  createdAt: string;
  updatedAt: string | null;
}
