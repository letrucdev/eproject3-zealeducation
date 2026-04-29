import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmCardImports } from '@spartan-ng/helm/card';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '@shared/components/data-table';
import { MyBatchExamResults, MyExamResultRow } from '../../models/candidate-portal-models';

@Directive({
  selector: '[testCell]',
  providers: [{ provide: DataTableCellDef, useExisting: TestCellDef }],
})
export class TestCellDef extends DataTableCellDef<MyExamResultRow> {
  override readonly appDataTableCell = input.required<string>({ alias: 'testCell' });

  static override ngTemplateContextGuard(
    _dir: TestCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<MyExamResultRow> {
    return true;
  }
}

@Component({
  selector: 'app-candidate-batch-tests-card',
  imports: [DataTable, TestCellDef, DatePipe, DecimalPipe, HlmCardImports, HlmBadgeImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader>
        <h3 hlmCardTitle>Tests &amp; Scores</h3>
        <p hlmCardDescription>Your examinations, scores, and grades.</p>
      </div>
      <div hlmCardContent>
        @if (isLoading()) {
          <p class="text-muted-foreground text-sm">Loading...</p>
        } @else {
          <app-data-table
            [columns]="columns"
            [rows]="rows()"
            [trackBy]="trackById"
            emptyMessage="No examinations yet."
          >
            <ng-template testCell="examName" let-row>
              <span class="font-medium">{{ row.examName }}</span>
            </ng-template>

            <ng-template testCell="examDate" let-row>
              {{ row.examDate | date: 'dd MMM yyyy' }}
            </ng-template>

            <ng-template testCell="score" let-row>
              @if (row.score != null) {
                <span class="font-medium">
                  {{ row.score | number: '1.0-2' }} / {{ row.maxScore }}
                </span>
              } @else {
                <span class="text-muted-foreground italic">Not graded</span>
              }
            </ng-template>

            <ng-template testCell="grade" let-row>
              {{ row.grade || '-' }}
            </ng-template>

            <ng-template testCell="result" let-row>
              @if (row.isPassed === true) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Passed</span>
              } @else if (row.isPassed === false) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Failed</span>
              } @else {
                <span class="text-muted-foreground italic">-</span>
              }
            </ng-template>
          </app-data-table>
        }
      </div>
    </section>
  `,
})
export class CandidateBatchTestsCard {
  readonly data = input<MyBatchExamResults | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly columns: DataTableColumn<MyExamResultRow>[] = [
    { key: 'examName', header: 'Exam', width: 'w-64' },
    { key: 'examDate', header: 'Date', width: 'w-32' },
    { key: 'score', header: 'Score', width: 'w-40', align: 'center' },
    { key: 'grade', header: 'Grade', width: 'w-24', align: 'center' },
    { key: 'result', header: 'Result', width: 'w-32', align: 'center' },
  ];

  protected readonly trackById = (row: MyExamResultRow): string => row.examId;

  protected rows(): MyExamResultRow[] {
    return this.data()?.rows ?? [];
  }
}
