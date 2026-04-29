import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { DataTableSortChange } from '@shared/components/data-table';
import { CandidatePortalService } from '../candidate-portal.service';
import { MyBatchListItem, MyBatchListQuery } from '../models/candidate-portal-models';
import { MyBatchesTable } from './components/my-batches-table';

@Component({
  selector: 'app-my-batches-page',
  imports: [HlmCardImports, MyBatchesTable],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2">
        <h1 class="text-2xl font-semibold tracking-tight">My Batches</h1>
        <p class="text-muted-foreground text-sm">
          Batches you are currently enrolled in.
        </p>
      </header>

      <section hlmCard>
        <div hlmCardContent>
          <app-my-batches-table
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
    </section>
  `,
})
export default class MyBatchesPage {
  private readonly _service = inject(CandidatePortalService);
  private readonly _router = inject(Router);

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  private readonly _params = computed<MyBatchListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.batchesQuery(this._params);

  protected onPageChanged(page: number): void {
    this.page.set(page);
  }

  protected onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  protected onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }

  protected onViewClicked(row: MyBatchListItem): void {
    void this._router.navigate(['/app/candidate/batches', row.batchId]);
  }
}
