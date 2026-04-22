import { AuditAction } from '../../../../core/models/audit-action';

export interface AuditLogListQuery {
  page: number;
  pageSize: number;
  search?: string;
  action?: AuditAction;
  userId?: string;
  tableName?: string;
  fromDate?: string;
  toDate?: string;
}
