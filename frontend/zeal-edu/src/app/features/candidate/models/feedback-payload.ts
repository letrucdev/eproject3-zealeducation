export interface SubmitFacultyFeedbackPayload {
  batchId: string;
  facultyId: string;
  rating: number;
  comment: string | null;
}

export interface SubmitCourseFeedbackPayload {
  batchId: string;
  rating: number;
  comment: string | null;
}

export interface SubmitGeneralFeedbackPayload {
  batchId: string;
  rating: number;
  comment: string | null;
}
