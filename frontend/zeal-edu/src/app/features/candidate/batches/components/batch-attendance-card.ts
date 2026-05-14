import { DatePipe, DecimalPipe } from '@angular/common';
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
import { AttendanceStatus } from '@core/models/attendance';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { fromIsoDate, toIsoDate, formatDateRange } from '@shared/utils/date-range';
import { MyAttendanceRow, MyBatchAttendance } from '../../models/candidate-portal-models';
import { BatchDateRange } from './batch-schedule-card';

@Directive({
  selector: '[attendanceCell]',
  providers: [{ provide: DataTableCellDef, useExisting: AttendanceCellDef }],
})
export class AttendanceCellDef extends DataTableCellDef<MyAttendanceRow> {
  override readonly appDataTableCell = input.required<string>({ alias: 'attendanceCell' });

  static override ngTemplateContextGuard(
    _dir: AttendanceCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<MyAttendanceRow> {
    return true;
  }
}

@Component({
  selector: 'app-candidate-batch-attendance-card',
  imports: [
    DataTable,
    AttendanceCellDef,
    DatePipe,
    DecimalPipe,
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
        <div class="flex flex-row flex-wrap items-start justify-between gap-3 w-full">
          <div class="flex flex-col gap-1">
            <h3 hlmCardTitle>Attendance</h3>
            <p hlmCardDescription>Your attendance and practical hours for this batch.</p>
          </div>
          @if (data(); as d) {
            <div class="flex flex-wrap items-center gap-2">
              <span hlmBadge class="bg-emerald-100 text-emerald-800">
                Present: {{ d.presentCount }} / {{ d.totalSessions }}
              </span>
              @if (d.lateCount > 0) {
                <span hlmBadge class="bg-amber-100 text-amber-800">Late: {{ d.lateCount }}</span>
              }
              @if (d.absentCount > 0) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Absent: {{ d.absentCount }}</span>
              }
              <span hlmBadge class="bg-sky-100 text-sky-800">
                Practical hours: {{ d.totalPracticalHours | number: '1.0-2' }}
              </span>
            </div>
          }
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
              aria-label="Filter attendance by date range"
            >
              <span>Select date range</span>
            </hlm-date-range-picker>
          </div>
        </div>
      </div>
      <div hlmCardContent>
        <app-data-table
          [columns]="columns"
          [page]="data()?.rows"
          [isLoading]="isLoading()"
          [pageSize]="pageSize()"
          [sortBy]="sortBy()"
          [sortDirection]="sortDirection()"
          [trackBy]="trackById"
          itemLabel="sessions"
          emptyMessage="No sessions yet."
          (pageChanged)="pageChanged.emit($event)"
          (pageSizeChanged)="pageSizeChanged.emit($event)"
          (sortChanged)="sortChanged.emit($event)"
        >
          <ng-template attendanceCell="sessionDate" let-row>
            {{ row.sessionDate | date: 'dd MMM yyyy' }}
          </ng-template>

          <ng-template attendanceCell="topic" let-row>
            @if (row.topic) {
              {{ row.topic }}
            } @else {
              <span class="italic text-muted-foreground">Not set</span>
            }
          </ng-template>

          <ng-template attendanceCell="status" let-row>
            @switch (row.attendanceStatus) {
              @case (statuses.Present) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Present</span>
              }
              @case (statuses.Late) {
                <span hlmBadge class="bg-amber-100 text-amber-800">Late</span>
              }
              @case (statuses.Absent) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Absent</span>
              }
              @case (statuses.Excused) {
                <span hlmBadge class="bg-sky-100 text-sky-800">Excused</span>
              }
              @default {
                <span class="text-muted-foreground italic">Not marked</span>
              }
            }
          </ng-template>

          <ng-template attendanceCell="practicalHours" let-row>
            @if (row.practicalHours != null) {
              {{ row.practicalHours | number: '1.0-2' }}
            } @else {
              -
            }
          </ng-template>

          <ng-template attendanceCell="remarks" let-row>
            {{ row.remarks || '-' }}
          </ng-template>
        </app-data-table>
      </div>
    </section>
  `,
})
export class CandidateBatchAttendanceCard {
  readonly data = input<MyBatchAttendance | null | undefined>(null);
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

  protected readonly statuses = AttendanceStatus;

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

  protected readonly columns: DataTableColumn<MyAttendanceRow>[] = [
    { key: 'sessionDate', header: 'Date', sortable: true, sortKey: 'sessionDate' },
    { key: 'topic', header: 'Topic', sortable: true, sortKey: 'topic' },
    { key: 'status', header: 'My Status', sortable: true, sortKey: 'status', align: 'center' },
    {
      key: 'practicalHours',
      header: 'Practical Hrs',
      sortable: true,
      sortKey: 'practicalHours',
      align: 'center',
    },
    { key: 'remarks', header: 'Remarks' },
  ];

  protected readonly trackById = (row: MyAttendanceRow): string => row.sessionId;
}
