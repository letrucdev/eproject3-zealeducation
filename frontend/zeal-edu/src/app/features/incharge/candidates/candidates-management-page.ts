import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { CandidatesService } from './candidates.service';
import { ApplyFineDialog, ApplyFineSubmit } from './components/apply-fine-dialog';
import { CandidateFilterBar, CandidateFilterValue } from './components/candidate-filter-bar';
import { CandidateTable } from './components/candidate-table';
import { CandidateUpdateDialog, CandidateUpdateSubmit } from './components/candidate-update-dialog';
import { ResetPasswordResultDialog } from './components/reset-password-result-dialog';
import { CandidateListItem } from './models/candidate-list-item';
import { CandidateListQuery } from './models/candidate-payload';
import { DataTableSortChange } from '@shared/components/data-table';
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-candidates-management-page',
  imports: [
    HlmCardImports,
    CandidateFilterBar,
    CandidateTable,
    CandidateUpdateDialog,
    ApplyFineDialog,
    ConfirmDialog,
    ResetPasswordResultDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidates-management-page.html',
})
export default class CandidatesManagementPage {
  private readonly _service = inject(CandidatesService);
  private readonly _router = inject(Router);

  protected readonly updateDialog = viewChild.required<CandidateUpdateDialog>('updateDialog');
  protected readonly fineDialog = viewChild.required<ApplyFineDialog>('fineDialog');
  protected readonly resetConfirm = viewChild.required<ConfirmDialog>('resetConfirm');
  protected readonly resetResultDialog = viewChild.required<ResetPasswordResultDialog>('resetResultDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<CandidateListQuery['status']>(null);
  protected readonly courseFilter = signal<string | null>(null);
  protected readonly batchFilter = signal<string | null>(null);
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly initialFilter: CandidateFilterValue = {
    search: '',
    status: null,
    course: null,
    batch: null,
  };

  protected readonly focusedCandidateId = signal<string | null>(null);

  private readonly _listParams = computed<CandidateListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    status: this.statusFilter(),
    courseId: this.courseFilter(),
    batchId: this.batchFilter(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly detailQuery = this._service.detailQuery(this.focusedCandidateId);
  protected readonly updateMutation = this._service.updateMutation();
  protected readonly applyFineMutation = this._service.applyFineMutation();
  protected readonly resetPasswordMutation = this._service.resetPasswordMutation();

  private _pendingResetTarget: CandidateListItem | null = null;

  constructor() {
    effect(() => {
      const detail = this.detailQuery.data();
      const currentId = this.focusedCandidateId();
      const isFetching = this.detailQuery.isFetching();
      if (!detail || currentId !== detail.candidateId || isFetching) return;

      untracked(() => {
        this.updateDialog().open(detail);
        this.focusedCandidateId.set(null);
      });
    });

    effect(() => {
      const error = this.detailQuery.error();
      if (!error) return;
      untracked(() => this.focusedCandidateId.set(null));
    });
  }

  onFilterChanged(value: CandidateFilterValue): void {
    this.search.set(value.search);
    this.statusFilter.set(value.status);
    this.courseFilter.set(value.course?.courseId ?? null);
    this.batchFilter.set(value.batch?.batchId ?? null);
    this.page.set(1);
  }

  onPageChanged(page: number): void {
    this.page.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }

  onViewClicked(row: CandidateListItem): void {
    void this._router.navigate(['/app/incharge/candidates', row.candidateId]);
  }

  onEditClicked(row: CandidateListItem): void {
    this.focusedCandidateId.set(row.candidateId);
  }

  onFineClicked(row: CandidateListItem): void {
    this.fineDialog().open({
      candidateId: row.candidateId,
      candidateCode: row.candidateCode,
      candidateName: row.fullName,
    });
  }

  onResetPasswordClicked(row: CandidateListItem): void {
    this._pendingResetTarget = row;
    this.resetConfirm().open({
      title: 'Reset password?',
      message: `A new temporary password will be generated for ${row.fullName} (${row.candidateCode}). The candidate will be required to change it on next login.`,
      confirmLabel: 'Reset password',
    });
  }

  onResetConfirmed(): void {
    const target = this._pendingResetTarget;
    if (!target) return;
    this._pendingResetTarget = null;
    this.resetPasswordMutation.mutate(
      { candidateId: target.candidateId },
      {
        onSuccess: (result) => {
          toast.success('Password reset successfully.');
          this.resetResultDialog().open({
            ...result,
            candidateName: target.fullName,
            candidateCode: target.candidateCode,
          });
        },
      },
    );
  }

  onResetCancelled(): void {
    this._pendingResetTarget = null;
  }

  onUpdateSubmitted(event: CandidateUpdateSubmit): void {
    this.updateMutation.mutate(event, {
      onSuccess: () => {
        toast.success('Candidate updated successfully.');
        this.updateDialog().close();
      },
    });
  }

  onApplyFineSubmitted(event: ApplyFineSubmit): void {
    this.applyFineMutation.mutate(event, {
      onSuccess: () => {
        toast.success('Fine applied successfully.');
        this.fineDialog().close();
      },
    });
  }
}
