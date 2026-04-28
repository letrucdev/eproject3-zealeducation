import { DatePipe } from '@angular/common';
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
import { lucideArrowLeft, lucideClipboardCheck } from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { BatchStatus } from '@core/models/batch-status';
import { ClassSession, ClassSessionStatus } from '@core/models/class-session';
import { DataTableSortChange } from '@shared/components/data-table';
import { FacultyService } from '../faculty.service';
import {
  FacultyExamCandidatesDialog,
} from '../exams/components/faculty-exam-candidates-dialog';
import {
  FacultyExamScoreFormDialog,
  FacultyExamScoreSubmit,
} from '../exams/components/faculty-exam-score-form-dialog';
import { FacultyExamTable } from '../exams/components/faculty-exam-table';
import {
  FacultyBatchEnrollmentsQuery,
  FacultyExaminationCandidate,
  FacultyExaminationSummary,
  FacultyExaminationsQuery,
} from '../models/faculty-models';
import { FacultyBatchEnrollmentsTable } from './components/faculty-batch-enrollments-table';
import { FacultySessionsTable } from './components/faculty-sessions-table';

@Component({
  selector: 'app-faculty-batch-detail-page',
  imports: [
    DatePipe,
    RouterLink,
    HlmBadgeImports,
    HlmButtonImports,
    HlmCardImports,
    HlmIconImports,
    HlmSkeletonImports,
    FacultySessionsTable,
    FacultyBatchEnrollmentsTable,
    FacultyExamTable,
    FacultyExamCandidatesDialog,
    FacultyExamScoreFormDialog,
  ],
  providers: [provideIcons({ lucideArrowLeft, lucideClipboardCheck })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'faculty-batch-detail-page.html',
})
export default class FacultyBatchDetailPage {
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _service = inject(FacultyService);

  protected readonly candidatesDialog = viewChild.required<FacultyExamCandidatesDialog>('candidatesDialog');
  protected readonly scoreDialog = viewChild.required<FacultyExamScoreFormDialog>('scoreDialog');

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });
  protected readonly currentBatchId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('id') ?? null;
  });

  protected readonly sessionsPage = signal(1);
  protected readonly sessionsPageSize = signal(10);
  protected readonly sessionsSortBy = signal<string | null>('sessionDate');
  protected readonly sessionsSortDirection = signal<'asc' | 'desc'>('asc');

  protected readonly enrollmentsPage = signal(1);
  protected readonly enrollmentsPageSize = signal(10);
  protected readonly enrollmentsSearch = signal('');
  protected readonly enrollmentsSortBy = signal<string | null>('enrollmentDate');
  protected readonly enrollmentsSortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly examsPage = signal(1);
  protected readonly examsPageSize = signal(10);
  protected readonly examsSortBy = signal<string | null>('examDate');
  protected readonly examsSortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly activeExamination = signal<FacultyExaminationSummary | null>(null);
  private readonly _activeExaminationId = computed<string | null>(
    () => this.activeExamination()?.examinationId ?? null,
  );
  private readonly _candidatesEnabled = computed(() => this._activeExaminationId() !== null);

  private readonly _sessionsParams = computed(() => ({
    page: this.sessionsPage(),
    pageSize: this.sessionsPageSize(),
    sortBy: this.sessionsSortBy() ?? undefined,
    sortDirection: this.sessionsSortDirection(),
  }));

  private readonly _enrollmentsParams = computed<FacultyBatchEnrollmentsQuery>(() => ({
    page: this.enrollmentsPage(),
    pageSize: this.enrollmentsPageSize(),
    search: this.enrollmentsSearch() || undefined,
    sortBy: this.enrollmentsSortBy() ?? undefined,
    sortDirection: this.enrollmentsSortDirection(),
  }));

  private readonly _examsParams = computed<FacultyExaminationsQuery>(() => ({
    page: this.examsPage(),
    pageSize: this.examsPageSize(),
    batchId: this.currentBatchId(),
    sortBy: this.examsSortBy() ?? undefined,
    sortDirection: this.examsSortDirection(),
  }));

  protected readonly detailQuery = this._service.batchDetailQuery(this.currentBatchId);
  protected readonly sessionsQuery = this._service.batchSessionsQuery(
    this.currentBatchId,
    this._sessionsParams,
  );
  protected readonly enrollmentsQuery = this._service.batchEnrollmentsQuery(
    this.currentBatchId,
    this._enrollmentsParams,
  );
  protected readonly examsQuery = this._service.examinationsQuery(this._examsParams);
  protected readonly candidatesQuery = this._service.examCandidatesQuery(
    this._activeExaminationId,
    this._candidatesEnabled,
  );

  protected readonly createExamResultMutation = this._service.createExamResultMutation();
  protected readonly updateExamResultMutation = this._service.updateExamResultMutation();

  protected readonly statuses = BatchStatus;
  protected readonly sessionStatuses = ClassSessionStatus;

  protected onTakeAttendance(session: ClassSession): void {
    const batchId = this.currentBatchId();
    if (!batchId) return;
    void this._router.navigate([
      '/app/faculty/batches',
      batchId,
      'sessions',
      session.sessionId,
    ]);
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

  protected onExamsPageChanged(page: number): void {
    this.examsPage.set(page);
  }

  protected onExamsPageSizeChanged(size: number): void {
    this.examsPageSize.set(size);
    this.examsPage.set(1);
  }

  protected onExamsSortChanged(change: DataTableSortChange): void {
    this.examsSortBy.set(change.sortBy);
    this.examsSortDirection.set(change.sortDirection);
    this.examsPage.set(1);
  }

  protected onEnterScores(exam: FacultyExaminationSummary): void {
    this.activeExamination.set(exam);
    this.candidatesDialog().open();
  }

  protected onCandidateSelected(candidate: FacultyExaminationCandidate): void {
    const exam = this.activeExamination();
    if (!exam) return;
    this.scoreDialog().open(exam, candidate);
  }

  protected onScoreSubmitted(event: FacultyExamScoreSubmit): void {
    if (event.candidate.resultId) {
      this.updateExamResultMutation.mutate(
        {
          resultId: event.candidate.resultId,
          payload: { score: event.score, grade: event.grade },
        },
        {
          onSuccess: () => {
            toast.success('Score updated successfully.');
            this.scoreDialog().close();
          },
        },
      );
    } else {
      this.createExamResultMutation.mutate(
        {
          examinationId: event.examinationId,
          payload: {
            enrollmentId: event.candidate.enrollmentId,
            score: event.score,
            grade: event.grade,
          },
        },
        {
          onSuccess: () => {
            toast.success('Score entered successfully.');
            this.scoreDialog().close();
          },
        },
      );
    }
  }
}
