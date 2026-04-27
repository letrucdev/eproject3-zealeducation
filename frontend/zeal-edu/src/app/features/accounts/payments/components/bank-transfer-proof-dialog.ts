import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { PaymentsService } from '../payments.service';

@Component({
  selector: 'app-bank-transfer-proof-dialog',
  imports: [HlmDialogImports, HlmButtonImports, HlmSpinnerImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'bank-transfer-proof-dialog.html',
})
export class BankTransferProofDialog {
  private readonly _payments = inject(PaymentsService);

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');

  protected readonly loading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly imageUrl = signal<string | null>(null);
  protected readonly receiptNumber = signal<string | null>(null);

  async open(transactionId: string, receiptNumber?: string): Promise<void> {
    this._reset();
    this.receiptNumber.set(receiptNumber ?? null);
    this.dlg().open();
    this.loading.set(true);
    try {
      const { objectUrl } = await this._payments.getBankTransferProofObjectUrl(transactionId);
      this.imageUrl.set(objectUrl);
    } catch {
      this.errorMessage.set('Failed to load the bank transfer proof.');
    } finally {
      this.loading.set(false);
    }
  }

  close(): void {
    this.dlg().close();
    this._reset();
  }

  protected onClose(): void {
    this.close();
  }

  private _reset(): void {
    const url = this.imageUrl();
    if (url) URL.revokeObjectURL(url);
    this.imageUrl.set(null);
    this.errorMessage.set(null);
    this.loading.set(false);
    this.receiptNumber.set(null);
  }
}
