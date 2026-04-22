import { AuditAction } from './audit-action';

export interface AuditLogListItem {
  id: string;
  userId: string;
  username: string;
  userFullName: string;
  tableName: string;
  recordId: string;
  action: AuditAction;
  oldValuePreview: string | null;
  newValuePreview: string | null;
  changedAt: string;
  ipAddress: string | null;
}
