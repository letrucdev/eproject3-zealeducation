import { ChangeDetectionStrategy, Component, computed, inject, viewChild } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { provideIcons } from '@ng-icons/core';
import { lucideArrowLeft } from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { CandidatesService } from './candidates.service';
import { ApplyFineDialog, ApplyFineSubmit } from './components/apply-fine-dialog';
import { CandidateEnrollmentsCard } from './components/candidate-enrollments-card';
import { CandidateInfoCard } from './components/candidate-info-card';
import {
  CandidateUpdateDialog,
  CandidateUpdateSubmit,
} from './components/candidate-update-dialog';
import { FeeDetailDialog } from './components/fee-detail-dialog';
import { FeeHistoryTable } from './components/fee-history-table';
import { CandidateFeeStructureSummary } from '@core/models/candidate-detail';

@Component({
  selector: 'app-candidate-detail-page',
  imports: [
    RouterLink,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
    CandidateInfoCard,
    CandidateEnrollmentsCard,
    FeeHistoryTable,
    FeeDetailDialog,
    CandidateUpdateDialog,
    ApplyFineDialog,
  ],
  providers: [provideIcons({ lucideArrowLeft })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-detail-page.html',
})
export default class CandidateDetailPage {
  private readonly _route = inject(ActivatedRoute);
  private readonly _service = inject(CandidatesService);

  protected readonly updateDialog = viewChild.required<CandidateUpdateDialog>('updateDialog');
  protected readonly fineDialog = viewChild.required<ApplyFineDialog>('fineDialog');
  protected readonly feeDetailDialog = viewChild.required<FeeDetailDialog>('feeDetailDialog');

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });
  protected readonly currentCandidateId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('id') ?? null;
  });

  protected readonly detailQuery = this._service.detailQuery(this.currentCandidateId);
  protected readonly updateMutation = this._service.updateMutation();
  protected readonly applyFineMutation = this._service.applyFineMutation();

  protected onEditClicked(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.updateDialog().open(detail);
  }

  protected onFineClicked(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.fineDialog().open({
      candidateId: detail.candidateId,
      candidateCode: detail.candidateCode,
      candidateName: detail.fullName,
    });
  }

  protected onViewFeeClicked(fee: CandidateFeeStructureSummary): void {
    this.feeDetailDialog().open(fee.feeId);
  }

  protected onUpdateSubmitted(event: CandidateUpdateSubmit): void {
    this.updateMutation.mutate(event, {
      onSuccess: () => {
        toast.success('Candidate updated successfully.');
        this.updateDialog().close();
      },
    });
  }

  protected onApplyFineSubmitted(event: ApplyFineSubmit): void {
    this.applyFineMutation.mutate(event, {
      onSuccess: () => {
        toast.success('Fine applied successfully.');
        this.fineDialog().close();
      },
    });
  }
}
