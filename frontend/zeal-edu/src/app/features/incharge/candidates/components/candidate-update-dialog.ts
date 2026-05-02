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
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { CandidateStatus } from '@core/models/candidate-status';
import { CandidateDetail } from '@core/models/candidate-detail';
import { CANDIDATE_STATUS_LABELS } from '../models/candidate-labels';
import { UpdateCandidatePayload } from '../models/candidate-payload';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';

export interface CandidateUpdateSubmit {
  candidateId: string;
  payload: UpdateCandidatePayload;
}

@Component({
  selector: 'app-candidate-update-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    HlmTextareaImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-update-dialog.html',
})
export class CandidateUpdateDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<CandidateUpdateSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly initial = signal<CandidateDetail | null>(null);

  protected readonly statuses = CandidateStatus;
  protected readonly statusLabel = (v: CandidateStatus): string => CANDIDATE_STATUS_LABELS[v];

  readonly form = this._fb.nonNullable.group({
    fullName: this._fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    email: this._fb.nonNullable.control('', [
      Validators.required,
      Validators.email,
      Validators.maxLength(100),
    ]),
    phone: this._fb.nonNullable.control('', [Validators.required, Validators.pattern(/^\d{10}$/)]),
    address: this._fb.nonNullable.control(''),
    emergencyContact: this._fb.nonNullable.control('', [Validators.maxLength(100)]),
    notes: this._fb.nonNullable.control(''),
    status: this._fb.nonNullable.control<CandidateStatus>(CandidateStatus.Active),
  });

  open(detail: CandidateDetail): void {
    this.initial.set(detail);
    this.form.reset({
      fullName: detail.fullName,
      email: detail.email,
      phone: detail.phone,
      address: detail.address ?? '',
      emergencyContact: detail.emergencyContact ?? '',
      notes: detail.notes ?? '',
      status: detail.status,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const detail = this.initial();
    if (!detail) return;

    const v = this.form.getRawValue();
    const payload: UpdateCandidatePayload = {
      fullName: v.fullName.trim(),
      email: v.email.trim(),
      phone: v.phone.trim(),
      address: v.address.trim() ? v.address.trim() : null,
      emergencyContact: v.emergencyContact.trim() ? v.emergencyContact.trim() : null,
      notes: v.notes.trim() ? v.notes.trim() : null,
      status: v.status,
    };

    this.submitted.emit({ candidateId: detail.candidateId, payload });
  }
}
