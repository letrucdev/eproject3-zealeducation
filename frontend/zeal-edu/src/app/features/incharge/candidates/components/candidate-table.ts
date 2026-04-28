import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideEye, lucideKeyRound, lucidePencil, lucideScale } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { CandidateStatus } from '@core/models/candidate-status';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { CandidateListItem } from '../models/candidate-list-item';

@Directive({
  selector: '[candidateCell]',
  providers: [{ provide: DataTableCellDef, useExisting: CandidateCellDef }],
})
export class CandidateCellDef extends DataTableCellDef<CandidateListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'candidateCell' });

  static override ngTemplateContextGuard(
    _dir: CandidateCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<CandidateListItem> {
    return true;
  }
}

@Component({
  selector: 'app-candidate-table',
  imports: [
    DataTable,
    CandidateCellDef,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideEye, lucidePencil, lucideScale, lucideKeyRound })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-table.html',
})
export class CandidateTable {
  readonly page = input<PaginatedList<CandidateListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly viewClicked = output<CandidateListItem>();
  readonly editClicked = output<CandidateListItem>();
  readonly fineClicked = output<CandidateListItem>();
  readonly resetPasswordClicked = output<CandidateListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly statuses = CandidateStatus;

  protected readonly columns: DataTableColumn<CandidateListItem>[] = [
    { key: 'candidateCode', header: 'Code', sortable: true, width: 'w-32' },
    { key: 'fullName', header: 'Full Name', sortable: true, width: 'w-56' },
    { key: 'contact', header: 'Contact', width: 'w-56' },
    { key: 'currentCourse', header: 'Course', width: 'w-56' },
    { key: 'currentBatch', header: 'Batch', width: 'w-36' },
    { key: 'status', header: 'Status', sortable: true, width: 'w-32', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-44', align: 'right' },
  ];

  protected readonly trackById = (row: CandidateListItem): string => row.candidateId;
}
