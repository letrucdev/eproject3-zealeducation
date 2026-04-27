import { ClassSessionStatus } from './class-session';

export enum AttendanceStatus {
  Present = 'Present',
  Absent = 'Absent',
  Late = 'Late',
  Excused = 'Excused',
}

export interface AttendanceRow {
  enrollmentId: string;
  candidateId: string;
  candidateCode: string;
  fullName: string;
  status: AttendanceStatus | null;
  remarks: string | null;
}

export interface SessionAttendance {
  sessionId: string;
  batchId: string;
  batchCode: string;
  sessionDate: string;
  startTime: string;
  endTime: string;
  topic: string | null;
  location: string | null;
  status: ClassSessionStatus;
  rows: AttendanceRow[];
}

export interface MarkAttendanceEntry {
  enrollmentId: string;
  status: AttendanceStatus;
  remarks: string | null;
}

export interface MarkAttendancePayload {
  entries: MarkAttendanceEntry[];
}
