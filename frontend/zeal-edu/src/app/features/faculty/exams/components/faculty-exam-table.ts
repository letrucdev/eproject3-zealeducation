import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideClipboardCheck } from '@ng-icons/lucide';
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
import { FacultyExaminationSummary } from '../../models/faculty-models';

@Directive({
  selector: '[facultyExamCell]',
  providers: [{ provide: DataTableCellDef, useExisting: FacultyExamCellDef }],
})
export class FacultyExamCellDef extends DataTableCellDef<FacultyExaminationSummary> {
  override readonly appDataTableCell = input.required<string>({ alias: 'facultyExamCell' });

  static override ngTemplateContextGuard(
    _dir: FacultyExamCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<FacultyExaminationSummary> {
    return true;
  }
}

@Component({
  selector: 'app-faculty-exam-table',
  imports: [
    DataTable,
    FacultyExamCellDef,
    DatePipe,
    DecimalPipe,
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
      itemLabel="examinations"
      emptyMessage="No examinations for active batches."
      (pageChanged)="pageChanged.emit($event)"
      (pageSizeChanged)="pageSizeChanged.emit($event)"
      (sortChanged)="sortChanged.emit($event)"
    >
      <ng-template facultyExamCell="examName" let-row>
        <span class="font-medium">{{ row.examName }}</span>
        @if (row.location) {
          <div class="text-muted-foreground text-xs">{{ row.location }}</div>
        }
      </ng-template>

      <ng-template facultyExamCell="batchCode" let-row>
        <div class="flex flex-col">
          <span class="font-mono">{{ row.batchCode }}</span>
          <span class="text-muted-foreground text-xs">{{ row.courseName }}</span>
        </div>
      </ng-template>

      <ng-template facultyExamCell="examDate" let-row>
        {{ row.examDate | date: 'dd MMM yyyy' }}
      </ng-template>

      <ng-template facultyExamCell="maxScore" let-row>
        {{ row.maxScore }}
      </ng-template>

      <ng-template facultyExamCell="averageScore" let-row>
        @if (row.averageScore != null) {
          {{ row.averageScore | number: '1.0-2' }}
        } @else {
          <span class="text-muted-foreground">–</span>
        }
      </ng-template>

      <ng-template facultyExamCell="minScore" let-row>
        @if (row.minScore != null) {
          {{ row.minScore | number: '1.0-2' }}
        } @else {
          <span class="text-muted-foreground">–</span>
        }
      </ng-template>

      <ng-template facultyExamCell="maxStudentScore" let-row>
        @if (row.maxStudentScore != null) {
          {{ row.maxStudentScore | number: '1.0-2' }}
        } @else {
          <span class="text-muted-foreground">–</span>
        }
      </ng-template>

      <ng-template facultyExamCell="resultCount" let-row>
        {{ row.resultCount }}
      </ng-template>

      <ng-template facultyExamCell="actions" let-row>
        <div class="flex items-center justify-end gap-1">
          <button
            hlmBtn
            variant="outline"
            size="sm"
            type="button"
            (click)="enterScoresClicked.emit(row)"
          >
            <ng-icon hlm name="lucideClipboardCheck" size="sm" />
            Enter score
          </button>
        </div>
      </ng-template>
    </app-data-table>
  `,
})
export class FacultyExamTable {
  readonly page = input<PaginatedList<FacultyExaminationSummary> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly enterScoresClicked = output<FacultyExaminationSummary>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly columns: DataTableColumn<FacultyExaminationSummary>[] = [
    { key: 'examName', header: 'Exam', sortable: true, width: 'w-56' },
    { key: 'batchCode', header: 'Batch', sortable: true, width: 'w-44' },
    { key: 'examDate', header: 'Date', sortable: true, width: 'w-32' },
    { key: 'maxScore', header: 'Max', sortable: true, align: 'center', width: 'w-20' },
    { key: 'averageScore', header: 'Avg', align: 'center', width: 'w-20' },
    { key: 'minScore', header: 'Min', align: 'center', width: 'w-20' },
    { key: 'maxStudentScore', header: 'Max', align: 'center', width: 'w-20' },
    { key: 'resultCount', header: 'Graded', align: 'center', width: 'w-20' },
    { key: 'actions', header: 'Actions', align: 'right', width: 'w-32' },
  ];

  protected readonly trackById = (row: FacultyExaminationSummary): string => row.examinationId;
}
