import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucideEye, lucidePencil, lucideTrash2, lucideUserPlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { BatchListItem } from '@core/models/batch-list-item';
import { BatchStatus } from '@core/models/batch-status';
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
  selector: '[batchCell]',
  providers: [{ provide: DataTableCellDef, useExisting: BatchCellDef }],
})
export class BatchCellDef extends DataTableCellDef<BatchListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'batchCell' });

  static override ngTemplateContextGuard(
    _dir: BatchCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<BatchListItem> {
    return true;
  }
}

@Component({
  selector: 'app-batch-table',
  imports: [
    DataTable,
    BatchCellDef,
    DatePipe,
    RouterLink,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideEye, lucidePencil, lucideTrash2, lucideUserPlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-table.html',
})
export class BatchTable {
  readonly page = input<PaginatedList<BatchListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly viewClicked = output<BatchListItem>();
  readonly editClicked = output<BatchListItem>();
  readonly assignFacultyClicked = output<BatchListItem>();
  readonly deleteClicked = output<BatchListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly statuses = BatchStatus;

  protected readonly columns: DataTableColumn<BatchListItem>[] = [
    { key: 'batchCode', header: 'Batch Code', sortable: true, width: 'w-36' },
    { key: 'courseName', header: 'Course', sortable: true, width: 'w-64' },
    { key: 'facultyName', header: 'Faculty', width: 'w-56' },
    { key: 'schedule', header: 'Schedule', sortable: true, sortKey: 'startDate', width: 'w-56' },
    { key: 'capacity', header: 'Capacity', width: 'w-32', align: 'center' },
    { key: 'status', header: 'Status', sortable: true, width: 'w-40', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-40', align: 'right' },
  ];

  protected readonly trackById = (row: BatchListItem): string => row.batchId;
}
