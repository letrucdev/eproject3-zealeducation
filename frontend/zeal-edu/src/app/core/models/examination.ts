export interface Examination {
  examinationId: string;
  batchId: string;
  examName: string;
  examDate: string;
  location: string | null;
  maxScore: number;
  passScore: number;
  scheduledById: string;
  scheduledByName: string;
  hasResults: boolean;
  createdAt: string;
}

export interface CreateExaminationPayload {
  examName: string;
  examDate: string;
  location: string | null;
  maxScore: number;
  passScore: number;
}

export interface UpdateExaminationPayload extends CreateExaminationPayload {}
