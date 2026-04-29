import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
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
    HlmButtonImports,
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
        <div class="flex flex-col gap-3 md:flex-row md:flex-wrap md:items-end mt-2">
          <div class="flex flex-col gap-1.5">
            <label hlmFieldLabel for="schedule-from-date">From</label>
            <input
              hlmInput
              id="schedule-from-date"
              type="date"
              class="w-full md:w-44"
              [value]="fromDate()"
              [max]="toDate() || null"
              (change)="onFromDateChange($event)"
            />
          </div>
          <div class="flex flex-col gap-1.5">
            <label hlmFieldLabel for="schedule-to-date">To</label>
            <input
              hlmInput
              id="schedule-to-date"
              type="date"
              class="w-full md:w-44"
              [value]="toDate()"
              [min]="fromDate() || null"
              (change)="onToDateChange($event)"
            />
          </div>
          @if (fromDate() || toDate()) {
            <button hlmBtn variant="secondary" size="sm" type="button" (click)="onClearRange()">
              Clear range
            </button>
          }
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
  readonly pageSize = input<number>(20);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);
  readonly fromDate = input<string>('');
  readonly toDate = input<string>('');

  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();
  readonly dateRangeChanged = output<BatchDateRange>();

  protected readonly sessionStatuses = ClassSessionStatus;

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

  protected onFromDateChange(event: Event): void {
    const next = (event.target as HTMLInputElement).value;
    const to = this.toDate();
    const adjustedTo = to && next && to < next ? next : to;
    this.dateRangeChanged.emit({ fromDate: next, toDate: adjustedTo });
  }

  protected onToDateChange(event: Event): void {
    const next = (event.target as HTMLInputElement).value;
    const from = this.fromDate();
    const adjustedFrom = from && next && from > next ? next : from;
    this.dateRangeChanged.emit({ fromDate: adjustedFrom, toDate: next });
  }

  protected onClearRange(): void {
    this.dateRangeChanged.emit({ fromDate: '', toDate: '' });
  }
}
