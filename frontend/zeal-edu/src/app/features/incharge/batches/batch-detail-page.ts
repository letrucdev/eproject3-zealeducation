import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { provideIcons } from '@ng-icons/core';
import { lucideArrowLeft } from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { ClassSession } from '@core/models/class-session';
import { BatchEnrollmentItem } from '@core/models/batch-enrollment';
import { BatchStatus } from '@core/models/batch-status';
import { ExamResult } from '@core/models/exam-result';
import { Examination } from '@core/models/examination';
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';
import { BatchesService } from './batches.service';
import {
  AssignCandidatesDialog,
  AssignCandidatesSubmit,
} from './components/assign-candidates-dialog';
import {
  BatchAssignFacultyDialog,
  BatchAssignFacultySubmit,
} from './components/batch-assign-faculty-dialog';
import { BatchEnrollmentsCard } from './components/batch-enrollments-card';
import { BatchExamScoresTrendChart } from './components/batch-exam-scores-trend-chart';
import { BatchExaminationsCard } from './components/batch-examinations-card';
import { BatchFormDialog, BatchFormSubmit } from './components/batch-form-dialog';
import { BatchGradeDistributionChart } from './components/batch-grade-distribution-chart';
import { BatchInfoCard } from './components/batch-info-card';
import { BatchSessionsCard } from './components/batch-sessions-card';
import {
  BulkSessionFormDialog,
  BulkSessionFormSubmit,
} from './components/bulk-session-form-dialog';
import {
  ExamResultFormDialog,
  ExamResultFormSubmit,
} from './components/exam-result-form-dialog';
import { ExamResultsDialog } from './components/exam-results-dialog';
import {
  ExaminationFormDialog,
  ExaminationFormSubmit,
} from './components/examination-form-dialog';
import { SessionFormDialog, SessionFormSubmit } from './components/session-form-dialog';
import {
  BatchEnrollmentsQuery,
  BatchExaminationsQuery,
  BatchSessionsQuery,
} from './models/batch-payload';
import { BatchExamScoresTrendRange } from './models/batch-exam-scores-trend';
import { ExamResultsQuery } from '@core/models/exam-result';
import { DataTableSortChange } from '@shared/components/data-table';

@Component({
  selector: 'app-batch-detail-page',
  imports: [
    RouterLink,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
    BatchInfoCard,
    BatchEnrollmentsCard,
    BatchSessionsCard,
    BatchExaminationsCard,
    BatchExamScoresTrendChart,
    BatchGradeDistributionChart,
    BatchFormDialog,
    AssignCandidatesDialog,
    BatchAssignFacultyDialog,
    SessionFormDialog,
    BulkSessionFormDialog,
    ExaminationFormDialog,
    ExamResultsDialog,
    ExamResultFormDialog,
    ConfirmDialog,
  ],
  providers: [provideIcons({ lucideArrowLeft })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-detail-page.html',
})
export default class BatchDetailPage {
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _service = inject(BatchesService);

  protected readonly updateDialog = viewChild.required<BatchFormDialog>('updateDialog');
  protected readonly assignDialog = viewChild.required<AssignCandidatesDialog>('assignDialog');
  protected readonly assignFacultyDialog =
    viewChild.required<BatchAssignFacultyDialog>('assignFacultyDialog');
  protected readonly sessionDialog = viewChild.required<SessionFormDialog>('sessionDialog');
  protected readonly bulkSessionDialog =
    viewChild.required<BulkSessionFormDialog>('bulkSessionDialog');
  protected readonly examinationDialog =
    viewChild.required<ExaminationFormDialog>('examinationDialog');
  protected readonly examResultsDialog =
    viewChild.required<ExamResultsDialog>('examResultsDialog');
  protected readonly examResultFormDialog =
    viewChild.required<ExamResultFormDialog>('examResultFormDialog');
  protected readonly confirmDialog = viewChild.required<ConfirmDialog>('confirmDialog');

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });
  protected readonly currentBatchId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('id') ?? null;
  });

  protected readonly enrollmentsPage = signal(1);
  protected readonly enrollmentsPageSize = signal(10);
  protected readonly enrollmentsSearch = signal('');
  protected readonly enrollmentsSortBy = signal<string | null>('enrollmentDate');
  protected readonly enrollmentsSortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly sessionsPage = signal(1);
  protected readonly sessionsPageSize = signal(10);
  protected readonly sessionsSortBy = signal<string | null>('sessionDate');
  protected readonly sessionsSortDirection = signal<'asc' | 'desc'>('asc');

  protected readonly examinationsPage = signal(1);
  protected readonly examinationsPageSize = signal(10);
  protected readonly examinationsSearch = signal('');
  protected readonly examinationsSortBy = signal<string | null>('examDate');
  protected readonly examinationsSortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly examScoresTrendRange = signal<BatchExamScoresTrendRange>(90);

  protected readonly activeExamination = signal<Examination | null>(null);
  protected readonly examResultsPage = signal(1);
  protected readonly examResultsPageSize = signal(10);
  protected readonly examResultsSortBy = signal<string | null>('gradedAt');
  protected readonly examResultsSortDirection = signal<'asc' | 'desc'>('desc');

  private readonly _enrollmentsParams = computed<BatchEnrollmentsQuery>(() => ({
    page: this.enrollmentsPage(),
    pageSize: this.enrollmentsPageSize(),
    search: this.enrollmentsSearch() || undefined,
    sortBy: this.enrollmentsSortBy() ?? undefined,
    sortDirection: this.enrollmentsSortDirection(),
  }));

  private readonly _sessionsParams = computed<BatchSessionsQuery>(() => ({
    page: this.sessionsPage(),
    pageSize: this.sessionsPageSize(),
    sortBy: this.sessionsSortBy() ?? undefined,
    sortDirection: this.sessionsSortDirection(),
  }));

  private readonly _examinationsParams = computed<BatchExaminationsQuery>(() => ({
    page: this.examinationsPage(),
    pageSize: this.examinationsPageSize(),
    search: this.examinationsSearch() || undefined,
    sortBy: this.examinationsSortBy() ?? undefined,
    sortDirection: this.examinationsSortDirection(),
  }));

  private readonly _activeExaminationId = computed<string | null>(
    () => this.activeExamination()?.examinationId ?? null,
  );

  private readonly _examResultsParams = computed<ExamResultsQuery>(() => ({
    page: this.examResultsPage(),
    pageSize: this.examResultsPageSize(),
    sortBy: this.examResultsSortBy() ?? undefined,
    sortDirection: this.examResultsSortDirection(),
  }));

  private readonly _examResultsEnabled = computed(
    () => this._activeExaminationId() !== null,
  );

  protected readonly detailQuery = this._service.detailQuery(this.currentBatchId);
  protected readonly enrollmentsQuery = this._service.enrollmentsQuery(
    this.currentBatchId,
    this._enrollmentsParams,
  );
  protected readonly sessionsQuery = this._service.sessionsQuery(
    this.currentBatchId,
    this._sessionsParams,
  );
  protected readonly examinationsQuery = this._service.examinationsQuery(
    this.currentBatchId,
    this._examinationsParams,
  );
  protected readonly examScoresTrendQuery = this._service.examScoresTrendQuery(
    this.currentBatchId,
    this.examScoresTrendRange,
  );
  protected readonly gradeDistributionQuery = this._service.gradeDistributionQuery(
    this.currentBatchId,
  );
  protected readonly examResultsQuery = this._service.examResultsQuery(
    this._activeExaminationId,
    this._examResultsParams,
    this._examResultsEnabled,
  );

  protected readonly updateMutation = this._service.updateMutation();
  protected readonly assignCandidatesMutation = this._service.assignCandidatesMutation();
  protected readonly assignFacultyMutation = this._service.assignFacultyMutation();
  protected readonly deleteBatchMutation = this._service.deleteMutation();
  protected readonly createSessionMutation = this._service.createSessionMutation();
  protected readonly createBulkSessionsMutation = this._service.createBulkSessionsMutation();
  protected readonly updateSessionMutation = this._service.updateSessionMutation();
  protected readonly deleteSessionMutation = this._service.deleteSessionMutation();
  protected readonly createExaminationMutation = this._service.createExaminationMutation();
  protected readonly updateExaminationMutation = this._service.updateExaminationMutation();
  protected readonly deleteExaminationMutation = this._service.deleteExaminationMutation();
  protected readonly overrideExamResultMutation = this._service.overrideExamResultMutation();

  private readonly _pendingDelete = signal<
    | { kind: 'session'; session: ClassSession }
    | { kind: 'examination'; examination: Examination }
    | { kind: 'batch' }
    | null
  >(null);

  protected readonly canMutate = computed(() => {
    const detail = this.detailQuery.data();
    if (!detail) return false;
    return detail.status !== BatchStatus.Completed && detail.status !== BatchStatus.Cancelled;
  });

  protected readonly hasSchedule = computed(() => {
    const detail = this.detailQuery.data();
    return (detail?.sessionCount ?? 0) > 0;
  });

  protected readonly canAssignFaculty = computed(() => this.canMutate() && this.hasSchedule());

  protected readonly canAddExamination = computed(() => this.canMutate() && this.hasSchedule());

  protected readonly canAddCandidate = computed(() => {
    const detail = this.detailQuery.data();
    if (!detail) return false;
    if (!this.canMutate()) return false;
    if (!this.hasSchedule()) return false;
    return detail.enrolledCount < detail.maxCapacity;
  });

  protected readonly isSubmittingSession = computed(
    () => this.createSessionMutation.isPending() || this.updateSessionMutation.isPending(),
  );

  protected readonly isSubmittingExamination = computed(
    () =>
      this.createExaminationMutation.isPending() ||
      this.updateExaminationMutation.isPending(),
  );

  protected readonly isSubmittingExamResult = computed(() =>
    this.overrideExamResultMutation.isPending(),
  );

  protected onEditBatch(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.updateDialog().openEdit(detail);
  }

  protected onAddCandidate(): void {
    const id = this.currentBatchId();
    if (!id) return;
    this.assignDialog().open(id);
  }

  protected onAssignFaculty(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.assignFacultyDialog().open(detail);
  }

  protected onDeleteBatch(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this._pendingDelete.set({ kind: 'batch' });
    this.confirmDialog().open({
      title: 'Delete batch',
      message: `This will permanently delete batch ${detail.batchCode}. This action cannot be undone.`,
      confirmLabel: 'Delete',
      destructive: true,
    });
  }

  protected onViewCandidate(item: BatchEnrollmentItem): void {
    void this._router.navigate(['/app/incharge/candidates', item.candidateId]);
  }

  protected onAddSession(): void {
    const id = this.currentBatchId();
    if (!id) return;
    this.sessionDialog().openCreate(id);
  }

  protected onBulkCreateSessions(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.bulkSessionDialog().open(detail);
  }

  protected onBulkSubmitted(event: BulkSessionFormSubmit): void {
    this.createBulkSessionsMutation.mutate(
      { batchId: event.batchId, payload: event.payload },
      {
        onSuccess: (res) => {
          const skipped = res.skippedDates.length;
          const message =
            skipped === 0
              ? `Created ${res.createdCount} session(s).`
              : `Created ${res.createdCount} session(s); skipped ${skipped} due to time conflict.`;
          toast.success(message);
          this.bulkSessionDialog().close();
        },
      },
    );
  }

  protected onEditSession(session: ClassSession): void {
    this.sessionDialog().openEdit(session);
  }

  protected onDeleteSession(session: ClassSession): void {
    this._pendingDelete.set({ kind: 'session', session });
    this.confirmDialog().open({
      title: 'Delete session',
      message: `This will delete the session on ${session.sessionDate}. This action cannot be undone.`,
      confirmLabel: 'Delete',
      destructive: true,
    });
  }

  protected onTakeAttendance(session: ClassSession): void {
    const batchId = this.currentBatchId();
    if (!batchId) return;
    void this._router.navigate([
      '/app/incharge/batches',
      batchId,
      'sessions',
      session.sessionId,
    ]);
  }

  protected onUpdateSubmitted(event: BatchFormSubmit): void {
    if (event.mode !== 'edit') return;
    this.updateMutation.mutate(
      { batchId: event.batchId, payload: event.payload },
      {
        onSuccess: () => {
          toast.success('Batch updated successfully.');
          this.updateDialog().close();
        },
      },
    );
  }

  protected onAssignSubmitted(event: AssignCandidatesSubmit): void {
    this.assignCandidatesMutation.mutate(
      { batchId: event.batchId, payload: { enrollmentIds: event.enrollmentIds } },
      {
        onSuccess: () => {
          toast.success('Candidates added to batch.');
          this.assignDialog().close();
        },
      },
    );
  }

  protected onAssignFacultySubmitted(event: BatchAssignFacultySubmit): void {
    this.assignFacultyMutation.mutate(event, {
      onSuccess: () => {
        toast.success('Faculty assignment updated.');
        this.assignFacultyDialog().close();
      },
    });
  }

  protected onSessionSubmitted(event: SessionFormSubmit): void {
    if (event.mode === 'create') {
      this.createSessionMutation.mutate(
        { batchId: event.batchId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Session created successfully.');
            this.sessionDialog().close();
          },
        },
      );
    } else {
      this.updateSessionMutation.mutate(
        { sessionId: event.sessionId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Session updated successfully.');
            this.sessionDialog().close();
          },
        },
      );
    }
  }

  protected onDeleteConfirmed(): void {
    const pending = this._pendingDelete();
    if (!pending) return;
    this._pendingDelete.set(null);

    if (pending.kind === 'session') {
      this.deleteSessionMutation.mutate(pending.session.sessionId, {
        onSuccess: () => toast.success('Session deleted successfully.'),
      });
      return;
    }

    if (pending.kind === 'examination') {
      this.deleteExaminationMutation.mutate(pending.examination.examinationId, {
        onSuccess: () => toast.success('Exam deleted successfully.'),
      });
      return;
    }

    const batchId = this.currentBatchId();
    if (!batchId) return;
    this.deleteBatchMutation.mutate(batchId, {
      onSuccess: () => {
        toast.success('Batch deleted successfully.');
        void this._router.navigate(['/app/incharge/batches']);
      },
    });
  }

  protected onDeleteCancelled(): void {
    this._pendingDelete.set(null);
  }

  protected onEnrollmentsSearchChanged(value: string): void {
    this.enrollmentsSearch.set(value);
    this.enrollmentsPage.set(1);
  }

  protected onEnrollmentsPageChanged(page: number): void {
    this.enrollmentsPage.set(page);
  }

  protected onEnrollmentsPageSizeChanged(size: number): void {
    this.enrollmentsPageSize.set(size);
    this.enrollmentsPage.set(1);
  }

  protected onEnrollmentsSortChanged(change: DataTableSortChange): void {
    this.enrollmentsSortBy.set(change.sortBy);
    this.enrollmentsSortDirection.set(change.sortDirection);
    this.enrollmentsPage.set(1);
  }

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

  protected onAddExamination(): void {
    const id = this.currentBatchId();
    if (!id) return;
    this.examinationDialog().openCreate(id);
  }

  protected onEditExamination(examination: Examination): void {
    this.examinationDialog().openEdit(examination);
  }

  protected onDeleteExamination(examination: Examination): void {
    this._pendingDelete.set({ kind: 'examination', examination });
    this.confirmDialog().open({
      title: 'Delete exam',
      message: `This will delete the exam "${examination.examName}" on ${examination.examDate}. This action cannot be undone.`,
      confirmLabel: 'Delete',
      destructive: true,
    });
  }

  protected onExaminationSubmitted(event: ExaminationFormSubmit): void {
    if (event.mode === 'create') {
      this.createExaminationMutation.mutate(
        { batchId: event.batchId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Exam created successfully.');
            this.examinationDialog().close();
          },
        },
      );
    } else {
      this.updateExaminationMutation.mutate(
        { examinationId: event.examinationId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Exam updated successfully.');
            this.examinationDialog().close();
          },
        },
      );
    }
  }

  protected onExaminationsSearchChanged(value: string): void {
    this.examinationsSearch.set(value);
    this.examinationsPage.set(1);
  }

  protected onExaminationsPageChanged(page: number): void {
    this.examinationsPage.set(page);
  }

  protected onExaminationsPageSizeChanged(size: number): void {
    this.examinationsPageSize.set(size);
    this.examinationsPage.set(1);
  }

  protected onExaminationsSortChanged(change: DataTableSortChange): void {
    this.examinationsSortBy.set(change.sortBy);
    this.examinationsSortDirection.set(change.sortDirection);
    this.examinationsPage.set(1);
  }

  protected onViewResults(exam: Examination): void {
    this.activeExamination.set(exam);
    this.examResultsPage.set(1);
    this.examResultsDialog().open();
  }

  protected onOverrideExamResult(result: ExamResult): void {
    const exam = this.activeExamination();
    if (!exam) return;
    this.examResultFormDialog().open(exam, result);
  }

  protected onExamResultSubmitted(event: ExamResultFormSubmit): void {
    this.overrideExamResultMutation.mutate(
      { resultId: event.resultId, payload: event.payload },
      {
        onSuccess: () => {
          toast.success('Exam result overridden successfully.');
          this.examResultFormDialog().close();
        },
      },
    );
  }

  protected onExamResultsPageChanged(page: number): void {
    this.examResultsPage.set(page);
  }

  protected onExamResultsPageSizeChanged(size: number): void {
    this.examResultsPageSize.set(size);
    this.examResultsPage.set(1);
  }

  protected onExamResultsSortChanged(change: DataTableSortChange): void {
    this.examResultsSortBy.set(change.sortBy);
    this.examResultsSortDirection.set(change.sortDirection);
    this.examResultsPage.set(1);
  }

  protected onExamScoresTrendRangeChanged(range: BatchExamScoresTrendRange): void {
    this.examScoresTrendRange.set(range);
  }
}
