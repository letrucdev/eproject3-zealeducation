import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideEye } from '@ng-icons/lucide';
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
  selector: '[facultyBatchCell]',
  providers: [{ provide: DataTableCellDef, useExisting: FacultyBatchCellDef }],
})
export class FacultyBatchCellDef extends DataTableCellDef<BatchListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'facultyBatchCell' });

  static override ngTemplateContextGuard(
    _dir: FacultyBatchCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<BatchListItem> {
    return true;
  }
}

@Component({
  selector: 'app-faculty-batch-table',
  imports: [
    DataTable,
    FacultyBatchCellDef,
    DatePipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideEye })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-data-table
      [columns]="columns"
      [page]="page()"
      [isLoading]="isLoading()"
      [pageSize]="pageSize()"
      [sortBy]="sortBy()"
      [sortDirection]="sortDirection()"
      [trackBy]="trackById"
      itemLabel="batches"
      emptyMessage="No batches assigned to you yet."
      (pageChanged)="pageChanged.emit($event)"
      (pageSizeChanged)="pageSizeChanged.emit($event)"
      (sortChanged)="sortChanged.emit($event)"
    >
      <ng-template facultyBatchCell="batchCode" let-row>
        <span class="font-mono font-medium">{{ row.batchCode }}</span>
      </ng-template>

      <ng-template facultyBatchCell="courseName" let-row>
        <span class="font-medium">{{ row.courseName }}</span>
      </ng-template>

      <ng-template facultyBatchCell="schedule" let-row>
        <div class="flex flex-col text-xs">
          <span>
            {{ row.startDate | date: 'dd MMM yyyy' }} -> {{ row.endDate | date: 'dd MMM yyyy' }}
          </span>
          @if (row.location) {
            <span class="text-muted-foreground">{{ row.location }}</span>
          }
        </div>
      </ng-template>

      <ng-template facultyBatchCell="capacity" let-row>
        {{ row.enrolledCount }} / {{ row.maxCapacity }}
      </ng-template>

      <ng-template facultyBatchCell="status" let-row>
        @switch (row.status) {
          @case (statuses.NeedsInstructor) {
            <span hlmBadge class="bg-amber-100 text-amber-800">Needs Instructor</span>
          }
          @case (statuses.Active) {
            <span hlmBadge class="bg-emerald-100 text-emerald-800">Active</span>
          }
          @case (statuses.Completed) {
            <span hlmBadge class="bg-slate-100 text-slate-800">Completed</span>
          }
          @case (statuses.Cancelled) {
            <span hlmBadge class="bg-rose-100 text-rose-800">Cancelled</span>
          }
        }
      </ng-template>

      <ng-template facultyBatchCell="actions" let-row>
        <div class="flex items-center justify-end gap-1">
          <button
            hlmBtn
            variant="ghost"
            size="sm"
            type="button"
            (click)="viewClicked.emit(row)"
            [attr.aria-label]="'View ' + row.batchCode"
          >
            <ng-icon hlm name="lucideEye" size="sm" />
          </button>
        </div>
      </ng-template>
    </app-data-table>
  `,
})
export class FacultyBatchTable {
  readonly page = input<PaginatedList<BatchListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly viewClicked = output<BatchListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly statuses = BatchStatus;

  protected readonly columns: DataTableColumn<BatchListItem>[] = [
    { key: 'batchCode', header: 'Batch Code', sortable: true, width: 'w-36' },
    { key: 'courseName', header: 'Course', sortable: true, width: 'w-64' },
    { key: 'schedule', header: 'Schedule', sortable: true, sortKey: 'startDate', width: 'w-56' },
    { key: 'capacity', header: 'Capacity', width: 'w-32', align: 'center' },
    { key: 'status', header: 'Status', sortable: true, width: 'w-40', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-24', align: 'right' },
  ];

  protected readonly trackById = (row: BatchListItem): string => row.batchId;
}
