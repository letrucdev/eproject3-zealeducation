import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  computed,
  input,
  output,
  viewChild,
} from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideShieldCheck } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { ExamResult } from '@core/models/exam-result';
import { Examination } from '@core/models/examination';
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
  selector: '[examResultCell]',
  providers: [{ provide: DataTableCellDef, useExisting: ExamResultCellDef }],
})
export class ExamResultCellDef extends DataTableCellDef<ExamResult> {
  override readonly appDataTableCell = input.required<string>({ alias: 'examResultCell' });

  static override ngTemplateContextGuard(
    _dir: ExamResultCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<ExamResult> {
    return true;
  }
}

@Component({
  selector: 'app-exam-results-dialog',
  imports: [
    DatePipe,
    DataTable,
    ExamResultCellDef,
    HlmDialogImports,
    HlmButtonImports,
    HlmIconImports,
    HlmBadgeImports,
  ],
  providers: [
    provideIcons({
      lucideShieldCheck,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'exam-results-dialog.html',
})
export class ExamResultsDialog {
  readonly examination = input<Examination | null>(null);
  readonly page = input<PaginatedList<ExamResult> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly overrideClicked = output<ExamResult>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  protected readonly columns: DataTableColumn<ExamResult>[] = [
    { key: 'candidateCode', header: 'Code', sortable: true, sortKey: 'candidateCode' },
    {
      key: 'candidateFullName',
      header: 'Candidate',
      sortable: true,
      sortKey: 'candidateFullName',
    },
    { key: 'score', header: 'Score', sortable: true, align: 'center' },
    { key: 'grade', header: 'Grade', align: 'center' },
    { key: 'status', header: 'Status', align: 'center' },
    { key: 'gradedByName', header: 'Graded By' },
    { key: 'gradedAt', header: 'Graded At', sortable: true, sortKey: 'gradedAt' },
    { key: 'actions', header: 'Actions', align: 'right' },
  ];

  protected readonly trackById = (row: ExamResult): string => row.resultId;

  protected readonly description = computed(() => {
    const exam = this.examination();
    const total = this.page()?.totalCount ?? 0;
    if (!exam) return '';
    return `${total} result(s) for "${exam.examName}". Max ${exam.maxScore}, Pass ${exam.passScore}.`;
  });

  open(): void {
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }
}
