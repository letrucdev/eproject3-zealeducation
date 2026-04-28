import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideEye } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import {
  FeeType,
  PaymentStatus,
  PaymentType,
} from '@features/accounts/payments/models/payment-enums';
import { paymentTypeLabels } from '@features/accounts/payments/models/payment-labels';
import { CandidateFeeStructureSummary } from '../models/candidate-detail';

@Component({
  selector: 'app-fee-history-table',
  imports: [
    DatePipe,
    VndPipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmCardImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideEye })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'fee-history-table.html',
})
export class FeeHistoryTable {
  readonly feeStructures = input.required<CandidateFeeStructureSummary[]>();
  readonly viewClicked = output<CandidateFeeStructureSummary>();

  protected readonly paymentStatuses = PaymentStatus;
  protected readonly paymentTypes = PaymentType;
  protected readonly feeTypes = FeeType;
  protected readonly paymentTypeLabels = paymentTypeLabels;
}
