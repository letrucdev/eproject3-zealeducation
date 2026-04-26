export enum ClassSessionStatus {
  Scheduled = 'Scheduled',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

export interface ClassSession {
  sessionId: string;
  batchId: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
  status: ClassSessionStatus;
  attendanceMarkedCount: number;
}

export interface CreateClassSessionPayload {
  sessionDate: string;
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
}

export interface UpdateClassSessionPayload {
  sessionDate: string;
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
  status: ClassSessionStatus;
}
