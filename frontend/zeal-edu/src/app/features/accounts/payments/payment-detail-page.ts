import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import {
  lucideArrowLeft,
  lucideCircleCheck,
  lucideDownload,
  lucideEye,
  lucideTriangleAlert,
} from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { BankTransferProofDialog } from '@shared/components/bank-transfer-proof-dialog/bank-transfer-proof-dialog';
import {
  ConfirmPaymentDialog,
  ConfirmPaymentDialogSubmit,
} from './components/confirm-payment-dialog';
import {
  InstallmentPlanDialog,
  InstallmentPlanDialogSubmit,
} from './components/installment-plan-dialog';
import { InstallmentPlanItem, PaymentTransactionItem } from '@core/models/fee-structure';
import {
  InstallmentStatus,
  PaymentMethod,
  PaymentStatus,
  PaymentType,
} from '@core/models/payment-enums';
import { paymentMethodLabels, paymentTypeLabels } from '@core/models/payment-labels';
import { PaymentsService } from '@core/services/payments.service';

@Component({
  selector: 'app-payment-detail-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    VndPipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmCardImports,
    HlmIconImports,
    HlmSelectImports,
    HlmSkeletonImports,
    BankTransferProofDialog,
    ConfirmDialog,
    ConfirmPaymentDialog,
    InstallmentPlanDialog,
  ],
  providers: [
    provideIcons({
      lucideArrowLeft,
      lucideDownload,
      lucideEye,
      lucideCircleCheck,
      lucideTriangleAlert,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'payment-detail-page.html',
})
export default class PaymentDetailPage {
  private readonly _route = inject(ActivatedRoute);
  private readonly _service = inject(PaymentsService);
  private readonly _destroyRef = inject(DestroyRef);

  protected readonly paymentTypes = PaymentType;
  protected readonly paymentStatuses = PaymentStatus;
  protected readonly installmentStatuses = InstallmentStatus;
  protected readonly paymentMethods = PaymentMethod;

  protected readonly paymentTypeLabels = paymentTypeLabels;
  protected readonly paymentMethodLabels = paymentMethodLabels;

  protected readonly paymentTypeOptions: { value: PaymentType; label: string }[] = [
    { value: PaymentType.FullPayment, label: paymentTypeLabels[PaymentType.FullPayment] },
    { value: PaymentType.Installment, label: paymentTypeLabels[PaymentType.Installment] },
  ];

  protected readonly paymentTypeControl = new FormControl<PaymentType>(PaymentType.NotSet, {
    nonNullable: true,
  });

  protected readonly paymentTypeOptionLabel = (value: PaymentType): string =>
    this.paymentTypeLabels[value];

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });

  protected readonly currentFeeId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('feeId') ?? null;
  });

  protected readonly detailQuery = this._service.detailQuery(this.currentFeeId);
  protected readonly setPaymentTypeMutation = this._service.setPaymentTypeMutation();
  protected readonly confirmPaymentMutation = this._service.confirmPaymentMutation();

  protected readonly installmentDialog =
    viewChild.required<InstallmentPlanDialog>('installmentDialog');
  protected readonly confirmPaymentDialog =
    viewChild.required<ConfirmPaymentDialog>('confirmPaymentDialog');
  protected readonly fullPaymentTypeConfirm =
    viewChild.required<ConfirmDialog>('fullPaymentTypeConfirm');
  protected readonly confirmPaymentSubmitConfirm = viewChild.required<ConfirmDialog>(
    'confirmPaymentSubmitConfirm',
  );
  protected readonly bankTransferProofDialog =
    viewChild.required<BankTransferProofDialog>('bankTransferProofDialog');

  private readonly _pendingConfirmation = signal<ConfirmPaymentDialogSubmit | null>(null);

  protected readonly today = new Date().toISOString().substring(0, 10);

  private readonly _downloadingReceiptId = signal<string | null>(null);
  protected readonly downloadingReceiptId = this._downloadingReceiptId.asReadonly();

  protected readonly canChangePaymentType = computed<boolean>(() => {
    const detail = this.detailQuery.data();
    if (!detail) return false;
    return detail.paymentType === PaymentType.NotSet || detail.amountPaid === 0;
  });

  constructor() {
    effect(() => {
      const detail = this.detailQuery.data();
      if (!detail) return;
      if (this.paymentTypeControl.value !== detail.paymentType) {
        this.paymentTypeControl.setValue(detail.paymentType, { emitEvent: false });
      }
      const shouldEnable = this.canChangePaymentType() && !this.setPaymentTypeMutation.isPending();
      if (shouldEnable && this.paymentTypeControl.disabled) {
        this.paymentTypeControl.enable({ emitEvent: false });
      } else if (!shouldEnable && this.paymentTypeControl.enabled) {
        this.paymentTypeControl.disable({ emitEvent: false });
      }
    });

    this.paymentTypeControl.valueChanges
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        const detail = this.detailQuery.data();
        if (!detail || value === detail.paymentType) return;
        this.paymentTypeControl.setValue(detail.paymentType, { emitEvent: false });
        if (value === PaymentType.FullPayment) {
          this.onSetFullPaymentClicked();
        } else if (value === PaymentType.Installment) {
          this.onSetInstallmentClicked();
        }
      });
  }

  protected isInstallmentOverdue(item: InstallmentPlanItem): boolean {
    return item.status !== InstallmentStatus.Paid && this.today > item.dueDate;
  }

  protected canPayInstallment(item: InstallmentPlanItem): boolean {
    const detail = this.detailQuery.data();
    if (!detail) return false;
    return detail.installmentPlans
      .filter((i) => i.installmentNo < item.installmentNo)
      .every((i) => i.status === InstallmentStatus.Paid);
  }

  protected onSetFullPaymentClicked(): void {
    this.fullPaymentTypeConfirm().open({
      title: 'Set as Full Payment?',
      message:
        'The candidate will be required to pay the full outstanding amount in a single transaction.',
      confirmLabel: 'Confirm',
    });
  }

  protected onFullPaymentConfirmed(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.setPaymentTypeMutation.mutate(
      { feeId: detail.feeId, payload: { paymentType: PaymentType.FullPayment, frequency: null } },
      {
        onSuccess: () => toast.success('Payment type set to Full Payment.'),
      },
    );
  }

  protected onSetInstallmentClicked(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    if (detail.durationWeeks == null || detail.enrollmentDate == null) {
      toast.error('This fee is missing course or enrollment data.');
      return;
    }
    this.installmentDialog().open({
      durationWeeks: detail.durationWeeks,
      totalFee: detail.totalFee,
      enrollmentDate: detail.enrollmentDate,
    });
  }

  protected onInstallmentPlanSubmitted(event: InstallmentPlanDialogSubmit): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.setPaymentTypeMutation.mutate(
      {
        feeId: detail.feeId,
        payload: { paymentType: PaymentType.Installment, frequency: event.frequency },
      },
      {
        onSuccess: (res) => {
          toast.success(`Installment plan saved (${res.installments.length} installments).`);
          this.installmentDialog().close();
        },
      },
    );
  }

  protected onPayInstallmentClicked(installment: InstallmentPlanItem): void {
    if (!this.canPayInstallment(installment)) return;
    this.confirmPaymentDialog().open({ mode: 'installment', installment });
  }

  protected onPayFullClicked(): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this.confirmPaymentDialog().open({
      mode: 'full',
      outstandingBalance: detail.outstandingBalance,
    });
  }

  protected onConfirmPaymentSubmitted(event: ConfirmPaymentDialogSubmit): void {
    const detail = this.detailQuery.data();
    if (!detail) return;
    this._pendingConfirmation.set(event);
    this.confirmPaymentSubmitConfirm().open({
      title: 'Confirm Payment?',
      message: 'Are you sure you want to confirm this payment? This action cannot be undone.',
      confirmLabel: 'Confirm',
    });
  }

  protected onConfirmPaymentReallyConfirmed(): void {
    const detail = this.detailQuery.data();
    const event = this._pendingConfirmation();
    if (!detail || !event) return;
    this._pendingConfirmation.set(null);
    this.confirmPaymentMutation.mutate(
      { feeId: detail.feeId, payload: event },
      {
        onSuccess: (res) => {
          toast.success(
            `Payment confirmed. Receipt: ${res.receiptNumber}` +
              (res.penaltyAmount > 0 ? ` (incl. penalty ${res.penaltyAmount.toFixed(2)})` : ''),
          );
          this.confirmPaymentDialog().close();
        },
      },
    );
  }

  protected onConfirmPaymentCancelled(): void {
    this._pendingConfirmation.set(null);
  }

  protected onViewBankTransferProofClicked(transaction: PaymentTransactionItem): void {
    void this.bankTransferProofDialog().open(transaction.id, transaction.receiptNumber);
  }

  protected async onDownloadReceiptClicked(transaction: PaymentTransactionItem): Promise<void> {
    if (this._downloadingReceiptId() === transaction.id) return;
    this._downloadingReceiptId.set(transaction.id);
    try {
      await this._service.downloadReceipt(transaction.id, `${transaction.receiptNumber}.pdf`);
    } finally {
      this._downloadingReceiptId.set(null);
    }
  }
}
