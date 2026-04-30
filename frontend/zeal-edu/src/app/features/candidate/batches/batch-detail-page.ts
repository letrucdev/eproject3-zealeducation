import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucideArrowLeft } from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { DataTableSortChange } from '@shared/components/data-table';
import { CourseMaterialsQuery, StudyMaterialListItem } from '@features/incharge/materials/models/material-payload';
import { CandidatePortalService } from '../candidate-portal.service';
import { FeedbackService } from '../feedback.service';
import { MyBatchAttendanceQuery, MyBatchSessionsQuery } from '../models/candidate-portal-models';
import {
  SubmitCourseFeedbackPayload,
  SubmitFacultyFeedbackPayload,
  SubmitGeneralFeedbackPayload,
} from '../models/feedback-payload';
import { CandidateBatchAttendanceCard } from './components/batch-attendance-card';
import { CandidateBatchFeedbackCard } from './components/batch-feedback-card';
import { CandidateBatchInfoCard } from './components/batch-info-card';
import { CandidateBatchMaterialsCard } from './components/batch-materials-card';
import { BatchDateRange, CandidateBatchScheduleCard } from './components/batch-schedule-card';
import { CandidateBatchTestsCard } from './components/batch-tests-card';
import { CourseFeedbackDialog } from './components/course-feedback-dialog';
import { FacultyFeedbackDialog } from './components/faculty-feedback-dialog';
import { GeneralFeedbackDialog } from './components/general-feedback-dialog';

@Component({
  selector: 'app-candidate-batch-detail-page',
  imports: [
    RouterLink,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
    CandidateBatchInfoCard,
    CandidateBatchScheduleCard,
    CandidateBatchAttendanceCard,
    CandidateBatchTestsCard,
    CandidateBatchMaterialsCard,
    CandidateBatchFeedbackCard,
    FacultyFeedbackDialog,
    CourseFeedbackDialog,
    GeneralFeedbackDialog,
  ],
  providers: [provideIcons({ lucideArrowLeft })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2">
        <a hlmBtn variant="ghost" size="sm" routerLink="/app/candidate/batches" class="self-start">
          <ng-icon hlm name="lucideArrowLeft" size="sm" />
          Back to My Batches
        </a>
        <h1 class="text-2xl font-semibold tracking-tight">Batch Detail</h1>
      </header>

      @if (detailQuery.isPending()) {
        <div class="grid gap-4">
          <hlm-skeleton class="h-40" />
          <hlm-skeleton class="h-32" />
          <hlm-skeleton class="h-40" />
        </div>
      } @else if (detailQuery.isError()) {
        <div class="rounded-md border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
          Failed to load batch.
        </div>
      } @else if (detailQuery.data(); as detail) {
        <app-candidate-batch-info-card [detail]="detail" />

        <app-candidate-batch-schedule-card
          [page]="sessionsQuery.data()"
          [isLoading]="sessionsQuery.isPending()"
          [pageSize]="sessionsPageSize()"
          [sortBy]="sessionsSortBy()"
          [sortDirection]="sessionsSortDirection()"
          [fromDate]="sessionsFromDate()"
          [toDate]="sessionsToDate()"
          (pageChanged)="onSessionsPageChanged($event)"
          (pageSizeChanged)="onSessionsPageSizeChanged($event)"
          (sortChanged)="onSessionsSortChanged($event)"
          (dateRangeChanged)="onSessionsDateRangeChanged($event)"
        />

        <app-candidate-batch-attendance-card
          [data]="attendanceQuery.data()"
          [isLoading]="attendanceQuery.isPending()"
          [pageSize]="attendancePageSize()"
          [sortBy]="attendanceSortBy()"
          [sortDirection]="attendanceSortDirection()"
          [fromDate]="attendanceFromDate()"
          [toDate]="attendanceToDate()"
          (pageChanged)="onAttendancePageChanged($event)"
          (pageSizeChanged)="onAttendancePageSizeChanged($event)"
          (sortChanged)="onAttendanceSortChanged($event)"
          (dateRangeChanged)="onAttendanceDateRangeChanged($event)"
        />

        <app-candidate-batch-tests-card
          [data]="examResultsQuery.data()"
          [isLoading]="examResultsQuery.isPending()"
        />

        <app-candidate-batch-materials-card
          [data]="materialsQuery.data()"
          [isLoading]="materialsQuery.isPending()"
          (pageChanged)="onMaterialsPageChanged($event)"
          (downloadClicked)="onMaterialDownload($event)"
        />

        <app-candidate-batch-feedback-card
          [detail]="detail"
          (facultyClicked)="onFacultyFeedbackClick()"
          (courseClicked)="onCourseFeedbackClick()"
          (generalClicked)="onGeneralFeedbackClick()"
        />
      }
    </section>

    <app-faculty-feedback-dialog
      #facultyDialog
      [submitting]="facultyMutation.isPending()"
      (submitted)="onFacultySubmitted($event)"
    />
    <app-course-feedback-dialog
      #courseDialog
      [submitting]="courseMutation.isPending()"
      (submitted)="onCourseSubmitted($event)"
    />
    <app-general-feedback-dialog
      #generalDialog
      [submitting]="generalMutation.isPending()"
      (submitted)="onGeneralSubmitted($event)"
    />
  `,
})
export default class CandidateBatchDetailPage {
  private readonly _route = inject(ActivatedRoute);
  private readonly _service = inject(CandidatePortalService);
  private readonly _feedbackService = inject(FeedbackService);

  protected readonly batchId = signal<string | null>(this._route.snapshot.paramMap.get('id'));

  protected readonly sessionsPage = signal(1);
  protected readonly sessionsPageSize = signal(10);
  protected readonly sessionsSortBy = signal<string | null>(null);
  protected readonly sessionsSortDirection = signal<'asc' | 'desc'>('asc');
  protected readonly sessionsFromDate = signal('');
  protected readonly sessionsToDate = signal('');

  protected readonly attendancePage = signal(1);
  protected readonly attendancePageSize = signal(10);
  protected readonly attendanceSortBy = signal<string | null>(null);
  protected readonly attendanceSortDirection = signal<'asc' | 'desc'>('asc');
  protected readonly attendanceFromDate = signal('');
  protected readonly attendanceToDate = signal('');

  protected readonly materialsPage = signal(1);
  protected readonly materialsPageSize = signal(12);

  private readonly _materialsParams = computed<CourseMaterialsQuery>(() => ({
    page: this.materialsPage(),
    pageSize: this.materialsPageSize(),
  }));

  private readonly _sessionsParams = computed<MyBatchSessionsQuery>(() => ({
    page: this.sessionsPage(),
    pageSize: this.sessionsPageSize(),
    sortBy: this.sessionsSortBy() ?? undefined,
    sortDirection: this.sessionsSortDirection(),
    fromDate: this.sessionsFromDate() || undefined,
    toDate: this.sessionsToDate() || undefined,
  }));

  private readonly _attendanceParams = computed<MyBatchAttendanceQuery>(() => ({
    page: this.attendancePage(),
    pageSize: this.attendancePageSize(),
    sortBy: this.attendanceSortBy() ?? undefined,
    sortDirection: this.attendanceSortDirection(),
    fromDate: this.attendanceFromDate() || undefined,
    toDate: this.attendanceToDate() || undefined,
  }));

  protected readonly detailQuery = this._service.batchDetailQuery(this.batchId);
  protected readonly sessionsQuery = this._service.batchSessionsQuery(
    this.batchId,
    this._sessionsParams,
  );
  protected readonly attendanceQuery = this._service.batchAttendanceQuery(
    this.batchId,
    this._attendanceParams,
  );
  protected readonly examResultsQuery = this._service.batchExamResultsQuery(this.batchId);
  protected readonly materialsQuery = this._service.myBatchMaterialsQuery(
    this.batchId,
    this._materialsParams,
  );

  protected readonly facultyMutation = this._feedbackService.submitFacultyMutation();
  protected readonly courseMutation = this._feedbackService.submitCourseMutation();
  protected readonly generalMutation = this._feedbackService.submitGeneralMutation();

  protected readonly facultyDialog = viewChild<FacultyFeedbackDialog>('facultyDialog');
  protected readonly courseDialog = viewChild<CourseFeedbackDialog>('courseDialog');
  protected readonly generalDialog = viewChild<GeneralFeedbackDialog>('generalDialog');

  protected onSessionsPageChanged(page: number): void {
    this.sessionsPage.set(page);
  }

  protected onSessionsPageSizeChanged(size: number): void {
    this.sessionsPageSize.set(size);
    this.sessionsPage.set(1);
  }

  protected onSessionsSortChanged(change: DataTableSortChange): void {
    this.sessionsSortBy.set(change.sortBy);
    this.sessionsSortDirection.set(change.sortDirection);
    this.sessionsPage.set(1);
  }

  protected onSessionsDateRangeChanged(range: BatchDateRange): void {
    this.sessionsFromDate.set(range.fromDate);
    this.sessionsToDate.set(range.toDate);
    this.sessionsPage.set(1);
  }

  protected onAttendancePageChanged(page: number): void {
    this.attendancePage.set(page);
  }

  protected onAttendancePageSizeChanged(size: number): void {
    this.attendancePageSize.set(size);
    this.attendancePage.set(1);
  }

  protected onAttendanceSortChanged(change: DataTableSortChange): void {
    this.attendanceSortBy.set(change.sortBy);
    this.attendanceSortDirection.set(change.sortDirection);
    this.attendancePage.set(1);
  }

  protected onAttendanceDateRangeChanged(range: BatchDateRange): void {
    this.attendanceFromDate.set(range.fromDate);
    this.attendanceToDate.set(range.toDate);
    this.attendancePage.set(1);
  }

  protected onMaterialsPageChanged(page: number): void {
    this.materialsPage.set(page);
  }

  protected async onMaterialDownload(material: StudyMaterialListItem): Promise<void> {
    try {
      const { blob, fileName } = await this._service.downloadMaterialFile(material.materialId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName || material.fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {
      toast.error('Failed to download file.');
    }
  }

  protected onFacultyFeedbackClick(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.facultyDialog()?.open(detail);
  }

  protected onCourseFeedbackClick(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.courseDialog()?.open(detail);
  }

  protected onGeneralFeedbackClick(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.generalDialog()?.open(detail);
  }

  protected onFacultySubmitted(payload: SubmitFacultyFeedbackPayload): void {
    this.facultyMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('Faculty feedback submitted.');
        this.facultyDialog()?.close();
      },
    });
  }

  protected onCourseSubmitted(payload: SubmitCourseFeedbackPayload): void {
    this.courseMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('Course feedback submitted.');
        this.courseDialog()?.close();
      },
    });
  }

  protected onGeneralSubmitted(payload: SubmitGeneralFeedbackPayload): void {
    this.generalMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('General feedback submitted.');
        this.generalDialog()?.close();
      },
    });
  }
}
