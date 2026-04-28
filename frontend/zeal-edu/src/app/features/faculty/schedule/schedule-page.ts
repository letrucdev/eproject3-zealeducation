import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucideRefreshCw } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { DataTableSortChange } from '@shared/components/data-table';
import { FacultyService } from '../faculty.service';
import { FacultyScheduleQuery } from '../models/faculty-models';
import {
  FacultyScheduleFilterBar,
  FacultyScheduleFilterValue,
} from './components/faculty-schedule-filter-bar';
import {
  FacultyScheduleAttendanceClick,
  FacultyScheduleTable,
} from './components/faculty-schedule-table';
import { UpcomingClassAlert } from './components/upcoming-class-alert';

@Component({
  selector: 'app-faculty-schedule-page',
  imports: [
    HlmButtonImports,
    HlmCardImports,
    HlmIconImports,
    FacultyScheduleFilterBar,
    FacultyScheduleTable,
    UpcomingClassAlert,
  ],
  providers: [provideIcons({ lucideRefreshCw })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'schedule-page.html',
})
export default class SchedulePage {
  private readonly _service = inject(FacultyService);
  private readonly _router = inject(Router);

  protected readonly fromDate = signal<string>(toIsoDate(new Date()));
  protected readonly toDate = signal<string>(toIsoDate(addDays(new Date(), 7)));
  protected readonly batchId = signal<string | null>(null);
  protected readonly courseId = signal<string | null>(null);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly sortBy = signal<string | null>('sessionDate');
  protected readonly sortDirection = signal<'asc' | 'desc'>('asc');

  protected readonly initialFilter: FacultyScheduleFilterValue = {
    fromDate: this.fromDate(),
    toDate: this.toDate(),
    course: null,
    batch: null,
  };

  private readonly _params = computed<FacultyScheduleQuery>(() => ({
    from: this.fromDate(),
    to: this.toDate(),
    page: this.page(),
    pageSize: this.pageSize(),
    batchId: this.batchId(),
    courseId: this.courseId(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly scheduleQuery = this._service.scheduleQuery(this._params);

  onFilterChanged(value: FacultyScheduleFilterValue): void {
    this.fromDate.set(value.fromDate);
    this.toDate.set(value.toDate);
    this.courseId.set(value.course?.courseId ?? null);
    this.batchId.set(value.batch?.batchId ?? null);
    this.page.set(1);
  }

  onPageChanged(page: number): void {
    this.page.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }

  onMarkAttendance(event: FacultyScheduleAttendanceClick): void {
    void this._router.navigate([
      '/app/faculty/batches',
      event.batchId,
      'sessions',
      event.sessionId,
    ]);
  }

  onRefresh(): void {
    void this.scheduleQuery.refetch();
  }
}

function toIsoDate(d: Date): string {
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function addDays(d: Date, days: number): Date {
  const next = new Date(d);
  next.setDate(next.getDate() + days);
  return next;
}
