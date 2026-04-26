import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { provideIcons } from '@ng-icons/core';
import { lucideDownload, lucideEye } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { BankTransferProofDialog } from '@features/accounts/payments/components/bank-transfer-proof-dialog';
import { PaymentTransactionItem } from '@features/accounts/payments/models/fee-structure';
import {
  InstallmentStatus,
  PaymentMethod,
  PaymentStatus,
  PaymentType,
} from '@features/accounts/payments/models/payment-enums';
import {
  paymentMethodLabels,
  paymentTypeLabels,
} from '@features/accounts/payments/models/payment-labels';
import { PaymentsService } from '@features/accounts/payments/payments.service';

@Component({
  selector: 'app-fee-detail-dialog',
  imports: [
    DatePipe,
    VndPipe,
    HlmDialogImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
    BankTransferProofDialog,
  ],
  providers: [provideIcons({ lucideDownload, lucideEye })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'fee-detail-dialog.html',
})
export class FeeDetailDialog {
  private readonly _payments = inject(PaymentsService);

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');
  protected readonly bankTransferProofDialog =
    viewChild.required<BankTransferProofDialog>('bankTransferProofDialog');

  protected readonly feeId = signal<string | null>(null);
  protected readonly detailQuery = this._payments.detailQuery(this.feeId);

  protected readonly installmentStatuses = InstallmentStatus;
  protected readonly paymentMethods = PaymentMethod;
  protected readonly paymentStatuses = PaymentStatus;
  protected readonly paymentTypes = PaymentType;
  protected readonly paymentMethodLabels = paymentMethodLabels;
  protected readonly paymentTypeLabels = paymentTypeLabels;

  protected readonly today = new Date().toISOString().substring(0, 10);

  private readonly _downloadingReceiptId = signal<string | null>(null);
  protected readonly downloadingReceiptId = this._downloadingReceiptId.asReadonly();

  open(feeId: string): void {
    this.feeId.set(feeId);
    this.dlg().open();
  }

  close(): void {
    this.dlg().close();
    this.feeId.set(null);
  }

  protected onClose(): void {
    this.close();
  }

  protected isInstallmentOverdue(dueDate: string, status: InstallmentStatus): boolean {
    return status !== InstallmentStatus.Paid && this.today > dueDate;
  }

  protected onViewProofClicked(tx: PaymentTransactionItem): void {
    void this.bankTransferProofDialog().open(tx.id, tx.receiptNumber);
  }

  protected async onDownloadReceiptClicked(tx: PaymentTransactionItem): Promise<void> {
    if (this._downloadingReceiptId() === tx.id) return;
    this._downloadingReceiptId.set(tx.id);
    try {
      await this._payments.downloadReceipt(tx.id, `${tx.receiptNumber}.pdf`);
    } finally {
      this._downloadingReceiptId.set(null);
    }
  }
}
