import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCalendarPlus,
  lucideClipboardCheck,
  lucideCirclePlus,
  lucidePencil,
  lucideTrash2,
} from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { ClassSession, ClassSessionStatus } from '@core/models/class-session';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';

@Directive({
  selector: '[sessionCell]',
  providers: [{ provide: DataTableCellDef, useExisting: SessionCellDef }],
})
export class SessionCellDef extends DataTableCellDef<ClassSession> {
  override readonly appDataTableCell = input.required<string>({ alias: 'sessionCell' });

  static override ngTemplateContextGuard(
    _dir: SessionCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<ClassSession> {
    return true;
  }
}

@Component({
  selector: 'app-batch-sessions-card',
  imports: [
    DatePipe,
    DataTable,
    SessionCellDef,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [
    provideIcons({
      lucideCalendarPlus,
      lucideClipboardCheck,
      lucideCirclePlus,
      lucidePencil,
      lucideTrash2,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-sessions-card.html',
})
export class BatchSessionsCard {
  readonly page = input<PaginatedList<ClassSession> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly canAddSession = input<boolean>(true);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly addClicked = output<void>();
  readonly bulkClicked = output<void>();
  readonly editClicked = output<ClassSession>();
  readonly deleteClicked = output<ClassSession>();
  readonly attendanceClicked = output<ClassSession>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly sessionStatuses = ClassSessionStatus;

  protected readonly columns: DataTableColumn<ClassSession>[] = [
    { key: 'sessionDate', header: 'Date', sortable: true, width: 'w-48' },
    { key: 'time', header: 'Time', width: 'w-36' },
    { key: 'topic', header: 'Topic', sortable: true, width: 'w-56' },
    { key: 'location', header: 'Location', sortable: true, width: 'w-40' },
    { key: 'attendance', header: 'Attendance', align: 'center', width: 'w-32' },
    { key: 'status', header: 'Status', sortable: true, align: 'center', width: 'w-32' },
    { key: 'actions', header: 'Actions', align: 'right', width: 'w-32' },
  ];

  protected readonly trackById = (row: ClassSession): string => row.sessionId;

  protected readonly trimSeconds = (time: string): string => time.substring(0, 5);
}
