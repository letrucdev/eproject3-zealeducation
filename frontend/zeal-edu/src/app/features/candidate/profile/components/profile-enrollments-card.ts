import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import {
  CandidateDetail,
  CandidateEnrollmentItem,
  EnrollmentStatus,
} from '@core/models/candidate-detail';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '@shared/components/data-table';

@Directive({
  selector: '[enrollmentSummaryCell]',
  providers: [{ provide: DataTableCellDef, useExisting: EnrollmentSummaryCellDef }],
})
export class EnrollmentSummaryCellDef extends DataTableCellDef<CandidateEnrollmentItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'enrollmentSummaryCell' });

  static override ngTemplateContextGuard(
    _dir: EnrollmentSummaryCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<CandidateEnrollmentItem> {
    return true;
  }
}

@Component({
  selector: 'app-candidate-profile-enrollments-card',
  imports: [
    DataTable,
    EnrollmentSummaryCellDef,
    DatePipe,
    RouterLink,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader>
        <h3 hlmCardTitle>Course Enrollments</h3>
        <p hlmCardDescription>Courses and batches you are enrolled in.</p>
      </div>
      <div hlmCardContent>
        <app-data-table
          [columns]="columns"
          [rows]="rows()"
          [trackBy]="trackById"
          emptyMessage="No enrollments yet."
        >
          <ng-template enrollmentSummaryCell="courseName" let-row>
            <span class="font-medium">{{ row.courseName }}</span>
            <div class="text-muted-foreground text-xs">{{ row.durationWeeks }} weeks</div>
          </ng-template>

          <ng-template enrollmentSummaryCell="batchCode" let-row>
            @if (row.batchCode && row.batchId) {
              <a
                hlmBtn
                variant="link"
                size="sm"
                class="h-auto! p-0! font-mono"
                [routerLink]="['/app/candidate/batches', row.batchId]"
              >
                {{ row.batchCode }}
              </a>
            } @else {
              <span class="text-muted-foreground italic">Not assigned</span>
            }
          </ng-template>

          <ng-template enrollmentSummaryCell="schedule" let-row>
            @if (row.batchStartDate && row.batchEndDate) {
              <span class="text-xs">
                {{ row.batchStartDate | date: 'dd MMM yyyy' }} →
                {{ row.batchEndDate | date: 'dd MMM yyyy' }}
              </span>
            } @else {
              -
            }
          </ng-template>

          <ng-template enrollmentSummaryCell="enrollmentDate" let-row>
            {{ row.enrollmentDate | date: 'dd MMM yyyy' }}
          </ng-template>

          <ng-template enrollmentSummaryCell="status" let-row>
            @switch (row.status) {
              @case (statuses.PendingAssignment) {
                <span hlmBadge class="bg-amber-100 text-amber-800">Pending</span>
              }
              @case (statuses.Enrolled) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Enrolled</span>
              }
              @case (statuses.Completed) {
                <span hlmBadge class="bg-slate-100 text-slate-800">Completed</span>
              }
              @case (statuses.Withdrawn) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Withdrawn</span>
              }
              @case (statuses.OnBreak) {
                <span hlmBadge class="bg-sky-100 text-sky-800">On Break</span>
              }
            }
          </ng-template>
        </app-data-table>
      </div>
    </section>
  `,
})
export class CandidateProfileEnrollmentsCard {
  readonly profile = input.required<CandidateDetail>();

  protected readonly statuses = EnrollmentStatus;

  protected readonly columns: DataTableColumn<CandidateEnrollmentItem>[] = [
    { key: 'courseName', header: 'Course', width: 'w-72' },
    { key: 'batchCode', header: 'Batch', width: 'w-40' },
    { key: 'schedule', header: 'Schedule', width: 'w-56' },
    { key: 'enrollmentDate', header: 'Enrolled On', width: 'w-32' },
    { key: 'status', header: 'Status', width: 'w-32', align: 'center' },
  ];

  protected readonly trackById = (row: CandidateEnrollmentItem): string => row.enrollmentId;

  protected rows(): CandidateEnrollmentItem[] {
    return this.profile().enrollments;
  }
}
