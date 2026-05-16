import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  computed,
  input,
  output,
} from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmDatePickerImports } from '@spartan-ng/helm/date-picker';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
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
import { fromIsoDate, toIsoDate, formatDateRange } from '@shared/utils/date-range';

export interface BatchDateRange {
  fromDate: string;
  toDate: string;
}

@Directive({
  selector: '[scheduleCell]',
  providers: [{ provide: DataTableCellDef, useExisting: ScheduleCellDef }],
})
export class ScheduleCellDef extends DataTableCellDef<ClassSession> {
  override readonly appDataTableCell = input.required<string>({ alias: 'scheduleCell' });

  static override ngTemplateContextGuard(
    _dir: ScheduleCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<ClassSession> {
    return true;
  }
}

@Component({
  selector: 'app-candidate-batch-schedule-card',
  imports: [
    DataTable,
    ScheduleCellDef,
    DatePipe,
    HlmCardImports,
    HlmBadgeImports,
    HlmDatePickerImports,
    HlmFieldImports,
    HlmInputImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader class="flex flex-col gap-3">
        <div class="flex flex-col gap-1">
          <h3 hlmCardTitle>Schedule</h3>
          <p hlmCardDescription>Class sessions for this batch.</p>
        </div>
        <div
          class="flex flex-col gap-3 md:flex-row md:flex-wrap md:items-end mt-2 justify-center items-center"
        >
          <div class="flex flex-col gap-1.5">
            <label hlmFieldLabel>Date range</label>
            <hlm-date-range-picker
              class="w-full md:w-72"
              [date]="dateRange()"
              [autoCloseOnEndSelection]="true"
              [formatDates]="formatDates"
              [showClear]="true"
              captionLayout="dropdown"
              (dateChange)="onDateRangeChange($event)"
              aria-label="Filter schedule by date range"
            >
              <span>Select date range</span>
            </hlm-date-range-picker>
          </div>
        </div>
      </div>
      <div hlmCardContent>
        <app-data-table
          [columns]="columns"
          [page]="page()"
          [isLoading]="isLoading()"
          [pageSize]="pageSize()"
          [sortBy]="sortBy()"
          [sortDirection]="sortDirection()"
          [trackBy]="trackById"
          itemLabel="sessions"
          emptyMessage="No sessions scheduled yet."
          (pageChanged)="pageChanged.emit($event)"
          (pageSizeChanged)="pageSizeChanged.emit($event)"
          (sortChanged)="sortChanged.emit($event)"
        >
          <ng-template scheduleCell="sessionDate" let-row>
            {{ row.sessionDate | date: 'dd MMM yyyy' }}
          </ng-template>

          <ng-template scheduleCell="time" let-row>
            {{ formatTime(row.startTime) }} – {{ formatTime(row.endTime) }}
          </ng-template>

          <ng-template scheduleCell="topic" let-row>
            @if (row.topic) {
              {{ row.topic }}
            } @else {
              <span class="italic text-muted-foreground">Not set</span>
            }
          </ng-template>

          <ng-template scheduleCell="location" let-row>
            {{ row.location || '-' }}
          </ng-template>

          <ng-template scheduleCell="status" let-row>
            @switch (row.status) {
              @case (sessionStatuses.Scheduled) {
                <span hlmBadge class="bg-sky-100 text-sky-800">Scheduled</span>
              }
              @case (sessionStatuses.Completed) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Completed</span>
              }
              @case (sessionStatuses.Cancelled) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Cancelled</span>
              }
            }
          </ng-template>
        </app-data-table>
      </div>
    </section>
  `,
})
export class CandidateBatchScheduleCard {
  readonly page = input<PaginatedList<ClassSession> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);
  readonly fromDate = input<string>('');
  readonly toDate = input<string>('');

  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();
  readonly dateRangeChanged = output<BatchDateRange>();

  protected readonly sessionStatuses = ClassSessionStatus;

  protected readonly dateRange = computed<[Date, Date] | undefined>(() => {
    const start = fromIsoDate(this.fromDate());
    const end = fromIsoDate(this.toDate());
    if (!start || !end) return undefined;
    return [start, end];
  });

  protected readonly formatDates = formatDateRange;

  protected onDateRangeChange(value: [Date, Date] | null): void {
    if (!value) {
      this.dateRangeChanged.emit({ fromDate: '', toDate: '' });
      return;
    }
    const [start, end] = value;
    this.dateRangeChanged.emit({ fromDate: toIsoDate(start), toDate: toIsoDate(end) });
  }

  protected readonly columns: DataTableColumn<ClassSession>[] = [
    { key: 'sessionDate', header: 'Date', sortable: true, width: 'w-32' },
    { key: 'time', header: 'Time', width: 'w-36' },
    { key: 'topic', header: 'Topic', sortable: true, width: 'w-72' },
    { key: 'location', header: 'Location', width: 'w-40' },
    { key: 'status', header: 'Status', sortable: true, width: 'w-32', align: 'center' },
  ];

  protected readonly trackById = (row: ClassSession): string => row.sessionId;

  protected formatTime(value: string): string {
    return value?.length >= 5 ? value.substring(0, 5) : value;
  }
}
