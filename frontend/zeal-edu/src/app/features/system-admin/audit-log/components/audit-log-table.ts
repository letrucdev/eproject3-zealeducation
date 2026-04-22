import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideEye } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { AuditAction } from '../../../../core/models/audit-action';
import { AuditLogListItem } from '../../../../core/models/audit-log-list-item';
import { PaginatedList } from '../../../../core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '../../../../shared/components/data-table';
import { getUserInitials } from '../../../../core/utils/user-initials';

@Directive({
  selector: '[auditLogCell]',
  providers: [{ provide: DataTableCellDef, useExisting: AuditLogCellDef }],
})
export class AuditLogCellDef extends DataTableCellDef<AuditLogListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'auditLogCell' });

  static override ngTemplateContextGuard(
    _dir: AuditLogCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<AuditLogListItem> {
    return true;
  }
}

@Component({
  selector: 'app-audit-log-table',
  imports: [
    DataTable,
    AuditLogCellDef,
    DatePipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideEye })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'audit-log-table.html',
})
export class AuditLogTable {
  readonly page = input<PaginatedList<AuditLogListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly getUserInitials = getUserInitials;

  readonly viewClicked = output<AuditLogListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();

  protected readonly columns: DataTableColumn<AuditLogListItem>[] = [
    { key: 'changedAt', header: 'Changed At', width: 'w-36' },
    { key: 'user', header: 'User', width: 'w-48' },
    { key: 'action', header: 'Action', width: 'w-40', align: 'center' },
    { key: 'tableName', header: 'Table', width: 'w-40' },
    { key: 'recordId', header: 'Record ID', width: 'w-56' },
    { key: 'ipAddress', header: 'IP Address', width: 'w-40' },
    { key: 'actions', header: 'Actions', width: 'w-28', align: 'right' },
  ];

  protected readonly trackById = (row: AuditLogListItem): string => row.id;

  protected valuePreview(row: AuditLogListItem): string {
    return row.newValuePreview ?? row.oldValuePreview ?? '';
  }

  protected actionBadgeClass(action: AuditAction): string {
    switch (action) {
      case AuditAction.INSERT:
        return 'bg-emerald-100 text-emerald-800';
      case AuditAction.UPDATE:
        return 'bg-sky-100 text-sky-800';
      case AuditAction.DELETE:
        return 'bg-rose-100 text-rose-800';
      case AuditAction.OVERRIDE:
        return 'bg-amber-100 text-amber-900';
      default:
        return '';
    }
  }
}
