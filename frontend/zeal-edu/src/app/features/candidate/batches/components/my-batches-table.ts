import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucideEye } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { BatchStatus } from '@core/models/batch-status';
import { PaginatedList } from '@core/models/paginated-list';
import { EnrollmentStatus } from '@core/models/candidate-detail';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { MyBatchListItem } from '../../models/candidate-portal-models';

@Directive({
  selector: '[myBatchCell]',
  providers: [{ provide: DataTableCellDef, useExisting: MyBatchCellDef }],
})
export class MyBatchCellDef extends DataTableCellDef<MyBatchListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'myBatchCell' });

  static override ngTemplateContextGuard(
    _dir: MyBatchCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<MyBatchListItem> {
    return true;
  }
}

@Component({
  selector: 'app-my-batches-table',
  imports: [
    DataTable,
    MyBatchCellDef,
    DatePipe,
    RouterLink,
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
      emptyMessage="You are not enrolled in any batch yet."
      (pageChanged)="pageChanged.emit($event)"
      (pageSizeChanged)="pageSizeChanged.emit($event)"
      (sortChanged)="sortChanged.emit($event)"
    >
      <ng-template myBatchCell="batchCode" let-row>
        <a
          hlmBtn
          variant="link"
          size="sm"
          class="p-0"
          [routerLink]="['/app/candidate/batches', row.batchId]"
        >
          {{ row.batchCode }}
        </a>
      </ng-template>

      <ng-template myBatchCell="courseName" let-row>
        <span class="font-medium">{{ row.courseName }}</span>
      </ng-template>

      <ng-template myBatchCell="facultyName" let-row>
        @if (row.facultyName) {
          {{ row.facultyName }}
        } @else {
          <span class="italic text-muted-foreground">Not assigned</span>
        }
      </ng-template>

      <ng-template myBatchCell="schedule" let-row>
        <div class="flex flex-col text-xs">
          <span>
            {{ row.startDate | date: 'dd MMM yyyy' }} → {{ row.endDate | date: 'dd MMM yyyy' }}
          </span>
          @if (row.location) {
            <span class="text-muted-foreground">{{ row.location }}</span>
          }
        </div>
      </ng-template>

      <ng-template myBatchCell="status" let-row>
        @switch (row.batchStatus) {
          @case (batchStatuses.NeedsInstructor) {
            <span hlmBadge class="bg-amber-100 text-amber-800">Needs Instructor</span>
          }
          @case (batchStatuses.Active) {
            <span hlmBadge class="bg-emerald-100 text-emerald-800">Active</span>
          }
          @case (batchStatuses.Completed) {
            <span hlmBadge class="bg-slate-100 text-slate-800">Completed</span>
          }
          @case (batchStatuses.Cancelled) {
            <span hlmBadge class="bg-rose-100 text-rose-800">Cancelled</span>
          }
        }
      </ng-template>

      <ng-template myBatchCell="enrollmentStatus" let-row>
        @switch (row.enrollmentStatus) {
          @case (enrollmentStatuses.PendingAssignment) {
            <span hlmBadge class="bg-amber-100 text-amber-800">Pending</span>
          }
          @case (enrollmentStatuses.Enrolled) {
            <span hlmBadge class="bg-emerald-100 text-emerald-800">Enrolled</span>
          }
          @case (enrollmentStatuses.Completed) {
            <span hlmBadge class="bg-slate-100 text-slate-800">Completed</span>
          }
          @case (enrollmentStatuses.Withdrawn) {
            <span hlmBadge class="bg-rose-100 text-rose-800">Withdrawn</span>
          }
          @case (enrollmentStatuses.OnBreak) {
            <span hlmBadge class="bg-sky-100 text-sky-800">On Break</span>
          }
        }
      </ng-template>

      <ng-template myBatchCell="actions" let-row>
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
export class MyBatchesTable {
  readonly page = input<PaginatedList<MyBatchListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly viewClicked = output<MyBatchListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly batchStatuses = BatchStatus;
  protected readonly enrollmentStatuses = EnrollmentStatus;

  protected readonly columns: DataTableColumn<MyBatchListItem>[] = [
    { key: 'batchCode', header: 'Batch Code', sortable: true, width: 'w-32' },
    { key: 'courseName', header: 'Course', sortable: true, sortKey: 'courseName', width: 'w-56' },
    { key: 'facultyName', header: 'Instructor', width: 'w-44' },
    { key: 'schedule', header: 'Schedule', sortable: true, sortKey: 'startDate', width: 'w-56' },
    { key: 'status', header: 'Batch', sortable: true, sortKey: 'status', width: 'w-36', align: 'center' },
    { key: 'enrollmentStatus', header: 'My Status', width: 'w-32', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-24', align: 'right' },
  ];

  protected readonly trackById = (row: MyBatchListItem): string => row.enrollmentId;
}
