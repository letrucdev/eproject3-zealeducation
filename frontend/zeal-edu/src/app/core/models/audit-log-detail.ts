import { AuditAction } from './audit-action';

export interface AuditLogDetail {
  id: string;
  userId: string;
  username: string;
  userFullName: string;
  tableName: string;
  recordId: string;
  action: AuditAction;
  oldValue: string | null;
  newValue: string | null;
  changedAt: string;
  ipAddress: string | null;
}
