import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideCheckCheck, lucideEye, lucideStar, lucideUndo2 } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { FEEDBACK_TYPE_BADGE_CLASSES } from '../models/feedback-labels';
import { FeedbackListItem, FeedbackType } from '../models/feedback-list-item';

@Directive({
  selector: '[feedbackCell]',
  providers: [{ provide: DataTableCellDef, useExisting: FeedbackCellDef }],
})
export class FeedbackCellDef extends DataTableCellDef<FeedbackListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'feedbackCell' });

  static override ngTemplateContextGuard(
    _dir: FeedbackCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<FeedbackListItem> {
    return true;
  }
}

@Component({
  selector: 'app-feedback-table',
  imports: [
    DataTable,
    FeedbackCellDef,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    DatePipe,
  ],
  providers: [provideIcons({ lucideEye, lucideCheckCheck, lucideUndo2, lucideStar })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'feedback-table.html',
})
export class FeedbackTable {
  readonly page = input<PaginatedList<FeedbackListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly viewClicked = output<FeedbackListItem>();
  readonly toggleProcessedClicked = output<FeedbackListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly typeBadgeClass = (type: FeedbackType): string =>
    FEEDBACK_TYPE_BADGE_CLASSES[type];

  protected readonly columns: DataTableColumn<FeedbackListItem>[] = [
    { key: 'createdAt', header: 'Submitted', sortable: true, width: 'w-36' },
    { key: 'candidate', header: 'Candidate', width: 'w-48' },
    { key: 'batch', header: 'Batch', width: 'w-40' },
    { key: 'type', header: 'Type', sortable: true, width: 'w-28' },
    { key: 'target', header: 'About', width: 'w-40' },
    { key: 'rating', header: 'Rating', sortable: true, width: 'w-24', align: 'center' },
    { key: 'comment', header: 'Comment', minWidth: 'w-64' },
    { key: 'isProcessed', header: 'Status', sortable: true, width: 'w-32', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-28', align: 'right' },
  ];

  protected readonly trackById = (row: FeedbackListItem): string => row.feedbackId;
}
