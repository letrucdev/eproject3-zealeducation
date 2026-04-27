import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmRadioGroupImports } from '@spartan-ng/helm/radio-group';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { InstallmentPlanItem } from '../models/fee-structure';
import { PaymentMethod } from '../models/payment-enums';
import { paymentMethodLabels } from '../models/payment-labels';

export type ConfirmPaymentMode = 'installment' | 'full';

export interface ConfirmPaymentDialogContext {
  mode: ConfirmPaymentMode;
  installment?: InstallmentPlanItem;
  outstandingBalance?: number;
}

export interface ConfirmPaymentDialogSubmit {
  installmentPlanId: string | null;
  paymentMethod: PaymentMethod;
  proofFile: File | null;
}

const PENALTY_RATE = 0.05;
const MAX_PROOF_BYTES = 5 * 1024 * 1024;
const ALLOWED_PROOF_TYPES = ['image/jpeg', 'image/png', 'image/webp'];
const PROOF_ACCEPT = ALLOWED_PROOF_TYPES.join(',');

@Component({
  selector: 'app-confirm-payment-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmButtonImports,
    HlmFieldImports,
    HlmRadioGroupImports,
    VndPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'confirm-payment-dialog.html',
})
export class ConfirmPaymentDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitted = output<ConfirmPaymentDialogSubmit>();
  readonly submitting = input(false);

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');

  protected readonly context = signal<ConfirmPaymentDialogContext | null>(null);

  protected readonly form = this._fb.nonNullable.group({
    paymentMethod: [PaymentMethod.Cash as PaymentMethod, Validators.required],
  });

  protected readonly selectedMethod = toSignal(this.form.controls.paymentMethod.valueChanges, {
    initialValue: this.form.controls.paymentMethod.value,
  });

  protected readonly methods: PaymentMethod[] = [PaymentMethod.Cash, PaymentMethod.BankTransfer];
  protected readonly PaymentMethod = PaymentMethod;
  protected readonly proofAccept = PROOF_ACCEPT;

  protected readonly proofFile = signal<File | null>(null);
  protected readonly proofPreviewUrl = signal<string | null>(null);
  protected readonly proofError = signal<string | null>(null);

  protected readonly canSubmit = computed<boolean>(() => {
    if (this.submitting()) return false;
    if (this.form.invalid) return false;
    if (this.selectedMethod() === PaymentMethod.BankTransfer) {
      return this.proofFile() !== null && this.proofError() === null;
    }
    return true;
  });

  protected readonly baseAmount = computed<number>(() => {
    const ctx = this.context();
    if (!ctx) return 0;
    if (ctx.mode === 'installment') return ctx.installment?.amountDue ?? 0;
    return ctx.outstandingBalance ?? 0;
  });

  protected readonly isLate = computed<boolean>(() => {
    const ctx = this.context();
    if (!ctx || ctx.mode !== 'installment' || !ctx.installment) return false;
    const today = new Date().toISOString().substring(0, 10);
    return today > ctx.installment.dueDate;
  });

  protected readonly penaltyAmount = computed<number>(() => {
    if (!this.isLate()) return 0;
    return Math.round(this.baseAmount() * PENALTY_RATE * 100) / 100;
  });

  protected readonly totalAmount = computed<number>(() => this.baseAmount() + this.penaltyAmount());

  open(context: ConfirmPaymentDialogContext): void {
    this.context.set(context);
    this.form.reset({ paymentMethod: PaymentMethod.Cash });
    this._resetProof();
    this.dlg().open();
  }

  close(): void {
    this.dlg().close();
    this._resetProof();
  }

  protected readonly methodLabel = (method: PaymentMethod): string => paymentMethodLabels[method];

  protected onProofFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    if (!file) {
      this._resetProof();
      return;
    }

    if (!ALLOWED_PROOF_TYPES.includes(file.type)) {
      this._setProofError('Only JPG, PNG, or WebP images are allowed.');
      input.value = '';
      return;
    }

    if (file.size > MAX_PROOF_BYTES) {
      this._setProofError('Image must be 5 MB or smaller.');
      input.value = '';
      return;
    }

    this._revokePreview();
    this.proofFile.set(file);
    this.proofPreviewUrl.set(URL.createObjectURL(file));
    this.proofError.set(null);
  }

  protected onClearProof(): void {
    this._resetProof();
  }

  protected onSubmit(): void {
    if (this.submitting()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const ctx = this.context();
    if (!ctx) return;

    const method = this.form.getRawValue().paymentMethod;
    const proof = this.proofFile();

    if (method === PaymentMethod.BankTransfer && !proof) {
      this.proofError.set('Please attach the bank transfer proof image.');
      return;
    }

    this.submitted.emit({
      installmentPlanId: ctx.mode === 'installment' ? (ctx.installment?.id ?? null) : null,
      paymentMethod: method,
      proofFile: method === PaymentMethod.BankTransfer ? proof : null,
    });
  }

  private _setProofError(message: string): void {
    this._revokePreview();
    this.proofFile.set(null);
    this.proofPreviewUrl.set(null);
    this.proofError.set(message);
  }

  private _resetProof(): void {
    this._revokePreview();
    this.proofFile.set(null);
    this.proofPreviewUrl.set(null);
    this.proofError.set(null);
  }

  private _revokePreview(): void {
    const url = this.proofPreviewUrl();
    if (url) URL.revokeObjectURL(url);
  }
}
