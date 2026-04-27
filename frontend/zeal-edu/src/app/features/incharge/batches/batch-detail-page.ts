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
import { BatchFormDialog, BatchFormSubmit } from './components/batch-form-dialog';
import { BatchInfoCard } from './components/batch-info-card';
import { BatchSessionsCard } from './components/batch-sessions-card';
import {
  BulkSessionFormDialog,
  BulkSessionFormSubmit,
} from './components/bulk-session-form-dialog';
import { SessionFormDialog, SessionFormSubmit } from './components/session-form-dialog';
import { BatchEnrollmentsQuery, BatchSessionsQuery } from './models/batch-payload';
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
    BatchFormDialog,
    AssignCandidatesDialog,
    BatchAssignFacultyDialog,
    SessionFormDialog,
    BulkSessionFormDialog,
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
  protected readonly confirmDialog = viewChild.required<ConfirmDialog>('confirmDialog');

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });
  protected readonly currentBatchId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('id') ?? null;
  });

  protected readonly enrollmentsPage = signal(1);
  protected readonly enrollmentsPageSize = signal(10);
  protected readonly enrollmentsSortBy = signal<string | null>('enrollmentDate');
  protected readonly enrollmentsSortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly sessionsPage = signal(1);
  protected readonly sessionsPageSize = signal(10);
  protected readonly sessionsSortBy = signal<string | null>('sessionDate');
  protected readonly sessionsSortDirection = signal<'asc' | 'desc'>('asc');

  private readonly _enrollmentsParams = computed<BatchEnrollmentsQuery>(() => ({
    page: this.enrollmentsPage(),
    pageSize: this.enrollmentsPageSize(),
    sortBy: this.enrollmentsSortBy() ?? undefined,
    sortDirection: this.enrollmentsSortDirection(),
  }));

  private readonly _sessionsParams = computed<BatchSessionsQuery>(() => ({
    page: this.sessionsPage(),
    pageSize: this.sessionsPageSize(),
    sortBy: this.sessionsSortBy() ?? undefined,
    sortDirection: this.sessionsSortDirection(),
  }));

  protected readonly detailQuery = this._service.detailQuery(this.currentBatchId);
  protected readonly enrollmentsQuery = this._service.enrollmentsQuery(
    this.currentBatchId,
    this._enrollmentsParams,
  );
  protected readonly sessionsQuery = this._service.sessionsQuery(
    this.currentBatchId,
    this._sessionsParams,
  );

  protected readonly updateMutation = this._service.updateMutation();
  protected readonly assignCandidatesMutation = this._service.assignCandidatesMutation();
  protected readonly assignFacultyMutation = this._service.assignFacultyMutation();
  protected readonly deleteBatchMutation = this._service.deleteMutation();
  protected readonly createSessionMutation = this._service.createSessionMutation();
  protected readonly createBulkSessionsMutation = this._service.createBulkSessionsMutation();
  protected readonly updateSessionMutation = this._service.updateSessionMutation();
  protected readonly deleteSessionMutation = this._service.deleteSessionMutation();

  private readonly _pendingDelete = signal<
    { kind: 'session'; session: ClassSession } | { kind: 'batch' } | null
  >(null);

  protected readonly canMutate = computed(() => {
    const detail = this.detailQuery.data();
    if (!detail) return false;
    return detail.status !== BatchStatus.Completed && detail.status !== BatchStatus.Cancelled;
  });

  protected readonly canAddCandidate = computed(() => {
    const detail = this.detailQuery.data();
    if (!detail) return false;
    if (!this.canMutate()) return false;
    return detail.enrolledCount < detail.maxCapacity;
  });

  protected readonly isSubmittingSession = computed(
    () => this.createSessionMutation.isPending() || this.updateSessionMutation.isPending(),
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
}
