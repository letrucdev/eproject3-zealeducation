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
import { SessionFormDialog, SessionFormSubmit } from './components/session-form-dialog';
import { BatchEnrollmentsQuery } from './models/batch-payload';

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
  protected readonly confirmDialog = viewChild.required<ConfirmDialog>('confirmDialog');

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });
  protected readonly currentBatchId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('id') ?? null;
  });

  protected readonly enrollmentsPage = signal(1);
  protected readonly enrollmentsPageSize = 10;

  private readonly _enrollmentsParams = computed<BatchEnrollmentsQuery>(() => ({
    page: this.enrollmentsPage(),
    pageSize: this.enrollmentsPageSize,
  }));

  protected readonly detailQuery = this._service.detailQuery(this.currentBatchId);
  protected readonly enrollmentsQuery = this._service.enrollmentsQuery(
    this.currentBatchId,
    this._enrollmentsParams,
  );
  protected readonly sessionsQuery = this._service.sessionsQuery(this.currentBatchId);

  protected readonly updateMutation = this._service.updateMutation();
  protected readonly assignCandidatesMutation = this._service.assignCandidatesMutation();
  protected readonly assignFacultyMutation = this._service.assignFacultyMutation();
  protected readonly deleteBatchMutation = this._service.deleteMutation();
  protected readonly createSessionMutation = this._service.createSessionMutation();
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
}
