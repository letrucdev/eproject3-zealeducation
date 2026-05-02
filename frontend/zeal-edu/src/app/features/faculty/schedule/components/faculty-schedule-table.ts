import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { ClassSessionStatus } from '@core/models/class-session';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { CLASS_SESSION_STATUS_LABELS } from '@core/models/session-labels';
import { FacultyScheduleItem } from '../../models/faculty-models';

export interface FacultyScheduleAttendanceClick {
  batchId: string;
  sessionId: string;
}

@Directive({
  selector: '[scheduleCell]',
  providers: [{ provide: DataTableCellDef, useExisting: ScheduleCellDef }],
})
export class ScheduleCellDef extends DataTableCellDef<FacultyScheduleItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'scheduleCell' });

  static override ngTemplateContextGuard(
    _dir: ScheduleCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<FacultyScheduleItem> {
    return true;
  }
}

@Component({
  selector: 'app-faculty-schedule-table',
  imports: [DataTable, ScheduleCellDef, DatePipe, HlmBadgeImports, HlmButtonImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'faculty-schedule-table.html',
})
export class FacultyScheduleTable {
  readonly page = input<PaginatedList<FacultyScheduleItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly markAttendanceClicked = output<FacultyScheduleAttendanceClick>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly sessionStatuses = ClassSessionStatus;
  protected readonly statusLabel = (s: ClassSessionStatus): string =>
    CLASS_SESSION_STATUS_LABELS[s];

  protected readonly columns: DataTableColumn<FacultyScheduleItem>[] = [
    { key: 'sessionDate', header: 'Date', sortable: true, width: 'w-44' },
    { key: 'time', header: 'Time', sortable: true, sortKey: 'startTime', width: 'w-32' },
    { key: 'batchCode', header: 'Batch', sortable: true, width: 'w-36' },
    { key: 'courseName', header: 'Course', sortable: true, width: 'w-48' },
    { key: 'topic', header: 'Topic', width: 'w-56' },
    { key: 'location', header: 'Location', width: 'w-40' },
    { key: 'status', header: 'Status', sortable: true, width: 'w-32', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-44', align: 'right' },
  ];

  protected readonly trackById = (row: FacultyScheduleItem): string => row.sessionId;
}
