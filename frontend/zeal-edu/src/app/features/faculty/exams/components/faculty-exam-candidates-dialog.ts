import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  input,
  output,
  viewChild,
} from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil, lucidePlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
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
import {
  FacultyExaminationCandidate,
  FacultyExaminationSummary,
} from '../../models/faculty-models';

@Directive({
  selector: '[facultyExamCandidateCell]',
  providers: [{ provide: DataTableCellDef, useExisting: FacultyExamCandidateCellDef }],
})
export class FacultyExamCandidateCellDef extends DataTableCellDef<FacultyExaminationCandidate> {
  override readonly appDataTableCell = input.required<string>({
    alias: 'facultyExamCandidateCell',
  });

  static override ngTemplateContextGuard(
    _dir: FacultyExamCandidateCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<FacultyExaminationCandidate> {
    return true;
  }
}

@Component({
  selector: 'app-faculty-exam-candidates-dialog',
  imports: [
    DataTable,
    DatePipe,
    FacultyExamCandidateCellDef,
    HlmBadgeImports,
    HlmButtonImports,
    HlmDialogImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucidePlus, lucidePencil })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'faculty-exam-candidates-dialog.html',
})
export class FacultyExamCandidatesDialog {
  readonly examination = input<FacultyExaminationSummary | null>(null);
  readonly page = input<PaginatedList<FacultyExaminationCandidate> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly rowSelected = output<FacultyExaminationCandidate>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  protected readonly columns: DataTableColumn<FacultyExaminationCandidate>[] = [
    { key: 'candidateCode', header: 'Code', sortable: true, width: 'w-44' },
    { key: 'candidateFullName', header: 'Candidate', sortable: true, width: 'w-44' },
    { key: 'score', header: 'Score', sortable: true, align: 'center', width: 'w-44' },
    { key: 'grade', header: 'Grade', align: 'center', width: 'w-44' },
    { key: 'status', header: 'Status', align: 'center', width: 'w-52' },
    { key: 'gradedAt', header: 'Graded At', sortable: true, width: 'w-44' },
    { key: 'actions', header: 'Actions', align: 'right' },
  ];

  protected readonly trackByEnrollmentId = (row: FacultyExaminationCandidate): string =>
    row.enrollmentId;

  open(): void {
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected actionTitle(row: FacultyExaminationCandidate): string {
    if (row.isOverridden) return 'Result has been overridden by Incharge';
    if (row.isFinalized) return 'Result has been finalized — ask Incharge to override';
    return row.resultId ? 'Update score' : 'Enter score';
  }
}
