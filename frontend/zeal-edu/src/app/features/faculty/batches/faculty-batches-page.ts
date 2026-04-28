import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { BatchListItem } from '@core/models/batch-list-item';
import { BatchStatus } from '@core/models/batch-status';
import { DataTableSortChange } from '@shared/components/data-table';
import { FacultyService } from '../faculty.service';
import { FacultyBatchListQuery } from '../models/faculty-models';
import {
  FacultyBatchFilterBar,
  FacultyBatchFilterValue,
} from './components/faculty-batch-filter-bar';
import { FacultyBatchTable } from './components/faculty-batch-table';

@Component({
  selector: 'app-faculty-batches-page',
  imports: [HlmCardImports, FacultyBatchFilterBar, FacultyBatchTable],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-6">
      <div>
        <h1 class="text-2xl font-semibold tracking-tight">My Batches</h1>
        <p class="text-muted-foreground text-sm">
          List of batches assigned to you.
        </p>
      </div>

      <section hlmCard>
        <div hlmCardContent class="flex flex-col gap-4">
          <app-faculty-batch-filter-bar
            [initial]="initialFilter"
            (filterChanged)="onFilterChanged($event)"
          />
          <app-faculty-batch-table
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
        </div>
      </section>
    </div>
  `,
})
export default class FacultyBatchesPage {
  private readonly _service = inject(FacultyService);
  private readonly _router = inject(Router);

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<BatchStatus | null>(null);
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly initialFilter: FacultyBatchFilterValue = { search: '', status: null };

  private readonly _params = computed<FacultyBatchListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    status: this.statusFilter(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.batchesListQuery(this._params);

  onFilterChanged(value: FacultyBatchFilterValue): void {
    this.search.set(value.search);
    this.statusFilter.set(value.status);
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

  onViewClicked(row: BatchListItem): void {
    void this._router.navigate(['/app/faculty/batches', row.batchId]);
  }
}
