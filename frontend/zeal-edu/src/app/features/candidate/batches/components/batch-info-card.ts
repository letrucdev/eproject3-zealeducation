import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { BatchStatus } from '@core/models/batch-status';
import { EnrollmentStatus } from '@features/incharge/candidates/models/candidate-detail';
import { MyBatchDetail } from '../../models/candidate-portal-models';

@Component({
  selector: 'app-candidate-batch-info-card',
  imports: [DatePipe, HlmCardImports, HlmBadgeImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader class="flex flex-row flex-wrap items-start justify-between gap-3">
        <div class="flex flex-col gap-1">
          <h2 hlmCardTitle class="font-mono">{{ detail().batchCode }}</h2>
          <p hlmCardDescription>{{ detail().courseName }}</p>
          <div class="mt-2 flex flex-wrap items-center gap-2">
            @switch (detail().status) {
              @case (batchStatuses.NeedsInstructor) {
                <span hlmBadge class="bg-amber-100 text-amber-800">Needs Instructor</span>
              }
              @case (batchStatuses.Active) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Active</span>
              }
              @case (batchStatuses.Completed) {
                <span hlmBadge class="bg-slate-100 text-slate-800">Completed</span>
              }
              @case (batchStatuses.Cancelled) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Cancelled</span>
              }
            }
            @switch (detail().enrollmentStatus) {
              @case (enrollmentStatuses.PendingAssignment) {
                <span hlmBadge class="bg-amber-100 text-amber-800">My status: Pending</span>
              }
              @case (enrollmentStatuses.Enrolled) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">My status: Enrolled</span>
              }
              @case (enrollmentStatuses.Completed) {
                <span hlmBadge class="bg-slate-100 text-slate-800">My status: Completed</span>
              }
              @case (enrollmentStatuses.Withdrawn) {
                <span hlmBadge class="bg-rose-100 text-rose-800">My status: Withdrawn</span>
              }
              @case (enrollmentStatuses.OnBreak) {
                <span hlmBadge class="bg-sky-100 text-sky-800">My status: On Break</span>
              }
            }
          </div>
        </div>
      </div>
      <div hlmCardContent class="grid gap-4 md:grid-cols-2">
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Instructor</span>
          @if (detail().facultyName) {
            <span class="font-medium">{{ detail().facultyName }}</span>
            <span class="text-muted-foreground text-xs">{{ detail().facultyCode }}</span>
          } @else {
            <span class="font-medium italic text-muted-foreground">Not assigned</span>
          }
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Duration</span>
          <span class="font-medium">{{ detail().courseDurationWeeks }} weeks</span>
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Start Date</span>
          <span class="font-medium">{{ detail().startDate | date: 'dd MMM yyyy' }}</span>
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">End Date</span>
          <span class="font-medium">{{ detail().endDate | date: 'dd MMM yyyy' }}</span>
        </div>
        <div class="flex flex-col md:col-span-2">
          <span class="text-xs uppercase text-muted-foreground">Location</span>
          <span class="font-medium">{{ detail().location || '-' }}</span>
        </div>
      </div>
    </section>
  `,
})
export class CandidateBatchInfoCard {
  readonly detail = input.required<MyBatchDetail>();

  protected readonly batchStatuses = BatchStatus;
  protected readonly enrollmentStatuses = EnrollmentStatus;
}
