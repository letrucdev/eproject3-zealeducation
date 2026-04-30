import { BatchStatus } from '@core/models/batch-status';

export const BATCH_STATUS_LABELS: Record<BatchStatus, string> = {
  [BatchStatus.NeedsInstructor]: 'Needs Instructor',
  [BatchStatus.Active]: 'Active',
  [BatchStatus.Completed]: 'Completed',
  [BatchStatus.Cancelled]: 'Cancelled',
};
