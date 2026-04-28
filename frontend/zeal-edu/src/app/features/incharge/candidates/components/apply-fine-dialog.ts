import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { ApplyFinePayload } from '../models/candidate-payload';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';

export interface ApplyFineSubmit {
  candidateId: string;
  payload: ApplyFinePayload;
}

interface FineTarget {
  candidateId: string;
  candidateCode: string;
  candidateName: string;
}

@Component({
  selector: 'app-apply-fine-dialog',
  imports: [
    ReactiveFormsModule,
    VndPipe,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmTextareaImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'apply-fine-dialog.html',
})
export class ApplyFineDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<ApplyFineSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly target = signal<FineTarget | null>(null);

  readonly form = this._fb.nonNullable.group({
    violationReason: this._fb.nonNullable.control('', [
      Validators.required,
      Validators.maxLength(500),
    ]),
    penaltyAmount: this._fb.nonNullable.control<number>(0, [
      Validators.required,
      Validators.min(1),
    ]),
  });

  open(target: FineTarget): void {
    this.target.set(target);
    this.form.reset({ violationReason: '', penaltyAmount: 0 });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const target = this.target();
    if (!target) return;

    const v = this.form.getRawValue();
    this.submitted.emit({
      candidateId: target.candidateId,
      payload: {
        violationReason: v.violationReason.trim(),
        penaltyAmount: Number(v.penaltyAmount),
      },
    });
  }
}
