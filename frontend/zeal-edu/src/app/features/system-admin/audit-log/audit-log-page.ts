import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { toast } from '@spartan-ng/brain/sonner';
import { AuditLogListItem } from '@core/models/audit-log-list-item';
import { AuditLogService } from './audit-log.service';
import { AuditLogDetailDialog } from './components/audit-log-detail-dialog';
import {
  AuditLogFilterBar,
  AuditLogFilterValue,
} from './components/audit-log-filter-bar';
import { AuditLogTable } from './components/audit-log-table';
import { AuditLogListQuery } from './models/audit-log-list-query';
import { DataTableSortChange } from '@shared/components/data-table';

@Component({
  selector: 'app-audit-log-page',
  imports: [AuditLogFilterBar, AuditLogTable, AuditLogDetailDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2">
        <h1 class="text-2xl font-semibold tracking-tight">Audit Log</h1>
        <p class="text-muted-foreground max-w-2xl text-sm">
          Review every tracked insert, update, delete, and override performed on critical data.
          Entries are immutable and retained for compliance and auditing.
        </p>
      </header>

      <app-audit-log-filter-bar
        [initial]="initialFilter"
        (filterChanged)="onFilterChanged($event)"
      />

      <app-audit-log-table
        [page]="listQuery.data()"
        [isLoading]="listQuery.isPending()"
        [pageSize]="pageSize()"
        [sortBy]="sortBy()"
        [sortDirection]="sortDirection()"
        (viewClicked)="onViewClicked($event)"
        (pageChanged)="onPageChanged($event)"
        (pageSizeChanged)="onPageSizeChanged($event)"
        (sortChanged)="onSortChanged($event)"
      />

      <app-audit-log-detail-dialog
        #detailDialog
        [detail]="detailQuery.data()"
        [isLoading]="detailQuery.isPending()"
      />
    </section>
  `,
})
export default class AuditLogPage {
  private readonly _service = inject(AuditLogService);

  protected readonly detailDialog = viewChild.required<AuditLogDetailDialog>('detailDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly actionFilter = signal<AuditLogFilterValue['action']>('');
  protected readonly tableNameFilter = signal('');
  protected readonly fromDate = signal('');
  protected readonly toDate = signal('');
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');
  protected readonly selectedId = signal<string | null>(null);

  protected readonly initialFilter: AuditLogFilterValue = {
    search: '',
    action: '',
    tableName: '',
    fromDate: '',
    toDate: '',
  };

  private readonly _listParams = computed<AuditLogListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    action: this.actionFilter() || undefined,
    tableName: this.tableNameFilter() || undefined,
    fromDate: this.fromDate() ? new Date(this.fromDate()).toISOString() : undefined,
    toDate: this.toDate() ? this._endOfDay(this.toDate()) : undefined,
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly detailQuery = this._service.detailQuery(this.selectedId);

  constructor() {
    effect(() => {
      const error = this.detailQuery.error();
      if (error) {
        untracked(() => {
          toast.error(this._extractError(error) ?? 'Failed to load audit log entry.');
          this.selectedId.set(null);
        });
      }
    });
  }

  onFilterChanged(value: AuditLogFilterValue): void {
    this.search.set(value.search);
    this.actionFilter.set(value.action);
    this.tableNameFilter.set(value.tableName);
    this.fromDate.set(value.fromDate);
    this.toDate.set(value.toDate);
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

  onViewClicked(row: AuditLogListItem): void {
    this.selectedId.set(row.id);
    this.detailDialog().open();
  }

  private _endOfDay(dateStr: string): string {
    const d = new Date(dateStr);
    d.setHours(23, 59, 59, 999);
    return d.toISOString();
  }

  private _extractError(err: HttpErrorResponse): string | undefined {
    const body = err.error as { message?: string } | string | null | undefined;
    if (!body) return undefined;
    if (typeof body === 'string') return body.trim() || undefined;
    return body.message?.trim();
  }
}
