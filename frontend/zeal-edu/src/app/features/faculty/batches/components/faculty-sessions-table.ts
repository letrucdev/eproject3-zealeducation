import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideClipboardCheck } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
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
  selector: '[facultySessionCell]',
  providers: [{ provide: DataTableCellDef, useExisting: FacultySessionCellDef }],
})
export class FacultySessionCellDef extends DataTableCellDef<ClassSession> {
  override readonly appDataTableCell = input.required<string>({ alias: 'facultySessionCell' });

  static override ngTemplateContextGuard(
    _dir: FacultySessionCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<ClassSession> {
    return true;
  }
}

@Component({
  selector: 'app-faculty-sessions-table',
  imports: [
    DataTable,
    FacultySessionCellDef,
    DatePipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideClipboardCheck })],
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
      itemLabel="sessions"
      emptyMessage="No sessions have been scheduled yet."
      (pageChanged)="pageChanged.emit($event)"
      (pageSizeChanged)="pageSizeChanged.emit($event)"
      (sortChanged)="sortChanged.emit($event)"
    >
      <ng-template facultySessionCell="sessionDate" let-row>
        <span class="font-medium">{{ row.sessionDate | date: 'EEE, dd MMM yyyy' }}</span>
      </ng-template>

      <ng-template facultySessionCell="time" let-row>
        <span class="font-mono text-xs">
          {{ trimSeconds(row.startTime) }} - {{ trimSeconds(row.endTime) }}
        </span>
      </ng-template>

      <ng-template facultySessionCell="topic" let-row>
        {{ row.topic || '-' }}
      </ng-template>

      <ng-template facultySessionCell="location" let-row>
        {{ row.location || '-' }}
      </ng-template>

      <ng-template facultySessionCell="attendance" let-row>
        @if (row.attendanceMarkedCount > 0) {
          <span hlmBadge class="bg-sky-100 text-sky-800">
            {{ row.attendanceMarkedCount }} marked
          </span>
        } @else {
          <span class="text-muted-foreground text-xs">Not yet</span>
        }
      </ng-template>

      <ng-template facultySessionCell="status" let-row>
        @switch (row.status) {
          @case (sessionStatuses.Scheduled) {
            <span hlmBadge class="bg-amber-100 text-amber-800">Scheduled</span>
          }
          @case (sessionStatuses.Completed) {
            <span hlmBadge class="bg-emerald-100 text-emerald-800">Completed</span>
          }
          @case (sessionStatuses.Cancelled) {
            <span hlmBadge class="bg-rose-100 text-rose-800">Cancelled</span>
          }
        }
      </ng-template>

      <ng-template facultySessionCell="actions" let-row>
        <div class="flex items-center justify-end gap-1">
          <button
            hlmBtn
            variant="ghost"
            size="sm"
            type="button"
            (click)="attendanceClicked.emit(row)"
            [attr.aria-label]="'Take attendance for session on ' + row.sessionDate"
            [disabled]="row.status === sessionStatuses.Cancelled"
          >
            <ng-icon hlm name="lucideClipboardCheck" size="sm" />
          </button>
        </div>
      </ng-template>
    </app-data-table>
  `,
})
export class FacultySessionsTable {
  readonly page = input<PaginatedList<ClassSession> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly attendanceClicked = output<ClassSession>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly sessionStatuses = ClassSessionStatus;

  protected readonly columns: DataTableColumn<ClassSession>[] = [
    { key: 'sessionDate', header: 'Date', sortable: true },
    { key: 'time', header: 'Time', width: 'w-32' },
    { key: 'topic', header: 'Topic', sortable: true },
    { key: 'location', header: 'Location', sortable: true, width: 'w-40' },
    { key: 'attendance', header: 'Attendance', align: 'center', width: 'w-32' },
    { key: 'status', header: 'Status', sortable: true, align: 'center', width: 'w-32' },
    { key: 'actions', header: 'Actions', align: 'right', width: 'w-24' },
  ];

  protected readonly trackById = (row: ClassSession): string => row.sessionId;

  protected trimSeconds(time: string): string {
    return time.length >= 5 ? time.slice(0, 5) : time;
  }
}
