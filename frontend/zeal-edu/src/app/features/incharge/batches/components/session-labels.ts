import { AttendanceStatus } from '@core/models/attendance';
import { ClassSessionStatus } from '@core/models/class-session';

export const CLASS_SESSION_STATUS_LABELS: Record<ClassSessionStatus, string> = {
  [ClassSessionStatus.Scheduled]: 'Scheduled',
  [ClassSessionStatus.Completed]: 'Completed',
  [ClassSessionStatus.Cancelled]: 'Cancelled',
};

export const ATTENDANCE_STATUS_LABELS: Record<AttendanceStatus, string> = {
  [AttendanceStatus.Present]: 'Present',
  [AttendanceStatus.Absent]: 'Absent',
  [AttendanceStatus.Late]: 'Late',
  [AttendanceStatus.Excused]: 'Excused',
};
