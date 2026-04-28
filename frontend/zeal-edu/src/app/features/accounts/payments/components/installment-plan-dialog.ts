import { DatePipe } from '@angular/common';
import { VndPipe } from '@shared/pipes/vnd-pipe';
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
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { InstallmentFrequency } from '../models/payment-enums';
import {
  PlannedInstallment,
  availableFrequencies,
  calculateInstallmentPreview,
  frequencyLabel,
} from '../utils/installment-calculator';
import { HlmRadioGroupImports } from '@spartan-ng/helm/radio-group';

export interface InstallmentPlanDialogContext {
  durationWeeks: number;
  totalFee: number;
  enrollmentDate: string;
}

export interface InstallmentPlanDialogSubmit {
  frequency: InstallmentFrequency;
}

@Component({
  selector: 'app-installment-plan-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmButtonImports,
    HlmRadioGroupImports,
    HlmSpinnerImports,
    VndPipe,
    DatePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'installment-plan-dialog.html',
})
export class InstallmentPlanDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitted = output<InstallmentPlanDialogSubmit>();
  readonly submitting = input(false);

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');

  protected readonly context = signal<InstallmentPlanDialogContext | null>(null);

  protected readonly form = this._fb.nonNullable.group({
    frequency: [InstallmentFrequency.Monthly as InstallmentFrequency, Validators.required],
  });

  protected readonly selectedFrequency = toSignal(this.form.controls.frequency.valueChanges, {
    initialValue: this.form.controls.frequency.value,
  });

  protected readonly availableOptions = computed<InstallmentFrequency[]>(() => {
    const ctx = this.context();
    return ctx ? availableFrequencies(ctx.durationWeeks) : [];
  });

  protected readonly preview = computed<PlannedInstallment[]>(() => {
    const ctx = this.context();
    if (!ctx) return [];
    return calculateInstallmentPreview(
      ctx.totalFee,
      ctx.durationWeeks,
      ctx.enrollmentDate,
      this.selectedFrequency(),
    );
  });

  open(context: InstallmentPlanDialogContext): void {
    this.context.set(context);
    const allowed = availableFrequencies(context.durationWeeks);
    const initial = allowed[0] ?? InstallmentFrequency.Monthly;
    this.form.reset({ frequency: initial });
    this.dlg().open();
  }

  close(): void {
    this.dlg().close();
  }

  protected describe(option: InstallmentFrequency): string {
    return frequencyLabel(option);
  }

  protected previewSummary(option: InstallmentFrequency): string {
    const ctx = this.context();
    if (!ctx) return '';
    const list = calculateInstallmentPreview(
      ctx.totalFee,
      ctx.durationWeeks,
      ctx.enrollmentDate,
      option,
    );
    if (list.length === 0) return 'Not allowed';
    const per = list[0].amountDue;
    return `${list.length} installments — first ${per.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }

  protected onSubmit(): void {
    if (this.submitting()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitted.emit({ frequency: this.form.controls.frequency.value });
  }
}
