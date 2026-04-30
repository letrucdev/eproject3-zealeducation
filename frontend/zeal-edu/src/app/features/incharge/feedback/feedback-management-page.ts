import { ChangeDetectionStrategy, Component, computed, inject, signal, viewChild } from '@angular/core';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { DataTableSortChange } from '@shared/components/data-table';
import {
  FeedbackFilterBar,
  FeedbackFilterValue,
  ProcessedView,
} from './components/feedback-filter-bar';
import { FeedbackTable } from './components/feedback-table';
import { FeedbackDetailDialog } from './components/feedback-detail-dialog';
import { FeedbackService } from './feedback.service';
import { FeedbackListItem, FeedbackType } from './models/feedback-list-item';
import { FeedbackListQuery } from './models/feedback-payload';

@Component({
  selector: 'app-feedback-management-page',
  imports: [HlmCardImports, FeedbackFilterBar, FeedbackTable, FeedbackDetailDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'feedback-management-page.html',
})
export default class FeedbackManagementPage {
  private readonly _service = inject(FeedbackService);

  protected readonly detailDialog = viewChild.required<FeedbackDetailDialog>('detailDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly typeFilter = signal<FeedbackType | null>(null);
  protected readonly batchFilter = signal<string | null>(null);
  protected readonly ratingFilter = signal<number | null>(null);
  protected readonly processedView = signal<ProcessedView>('unprocessed');
  protected readonly sortBy = signal<string | null>('createdAt');
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly initialFilter: FeedbackFilterValue = {
    search: '',
    type: null,
    batch: null,
    rating: null,
    processedView: 'unprocessed',
  };

  private readonly _isProcessedQueryParam = computed<boolean | null>(() => {
    switch (this.processedView()) {
      case 'unprocessed':
        return false;
      case 'processed':
        return true;
      case 'all':
        return null;
    }
  });

  private readonly _listParams = computed<FeedbackListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    type: this.typeFilter(),
    batchId: this.batchFilter(),
    rating: this.ratingFilter(),
    isProcessed: this._isProcessedQueryParam(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly setProcessedMutation = this._service.setProcessedMutation();

  onFilterChanged(value: FeedbackFilterValue): void {
    this.search.set(value.search);
    this.typeFilter.set(value.type);
    this.batchFilter.set(value.batch?.batchId ?? null);
    this.ratingFilter.set(value.rating);
    this.processedView.set(value.processedView);
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

  onViewClicked(row: FeedbackListItem): void {
    this.detailDialog().open(row);
  }

  onToggleProcessedClicked(row: FeedbackListItem): void {
    const next = !row.isProcessed;
    this.setProcessedMutation.mutate(
      { feedbackId: row.feedbackId, isProcessed: next },
      {
        onSuccess: () => {
          toast.success(next ? 'Marked as processed.' : 'Marked as unprocessed.');
          this.detailDialog().close();
        },
      },
    );
  }
}
