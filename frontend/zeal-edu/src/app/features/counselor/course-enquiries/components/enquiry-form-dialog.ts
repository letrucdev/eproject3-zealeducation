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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { DigitsOnlyDirective } from '../../../../shared/directives/digits-only.directive';
import { CourseEnquiryDetail } from '../../../../core/models/course-enquiry-detail';
import { EnquirySource } from '../../../../core/models/enquiry-source';
import { EnquiryStatus } from '../../../../core/models/enquiry-status';
import {
  CreateEnquiryPayload,
  UpdateEnquiryPayload,
} from '../models/course-enquiry-payload';
import { ENQUIRY_SOURCE_LABELS, ENQUIRY_STATUS_LABELS } from './enquiry-labels';

export type EnquiryFormMode = 'create' | 'edit';

export interface EnquiryFormSubmitCreate {
  mode: 'create';
  payload: CreateEnquiryPayload;
}
export interface EnquiryFormSubmitUpdate {
  mode: 'edit';
  enquiryId: string;
  payload: UpdateEnquiryPayload;
}
export type EnquiryFormSubmit = EnquiryFormSubmitCreate | EnquiryFormSubmitUpdate;

@Component({
  selector: 'app-enquiry-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    DigitsOnlyDirective,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-form-dialog.html',
})
export class EnquiryFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<EnquiryFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly sources = EnquirySource;
  protected readonly statuses = EnquiryStatus;

  protected readonly sourceLabel = (value: EnquirySource): string =>
    ENQUIRY_SOURCE_LABELS[value];

  protected readonly statusLabel = (value: EnquiryStatus): string =>
    ENQUIRY_STATUS_LABELS[value];

  readonly mode = signal<EnquiryFormMode>('create');
  readonly initial = signal<CourseEnquiryDetail | null>(null);

  readonly isEdit = computed(() => this.mode() === 'edit');

  readonly form = this._fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    phone: [
      '',
      [Validators.required, Validators.minLength(10), Validators.maxLength(10), Validators.pattern(/^\d+$/)],
    ],
    email: ['', [Validators.email, Validators.maxLength(100)]],
    courseInterested: ['', [Validators.required, Validators.maxLength(150)]],
    source: [EnquirySource.WalkIn, [Validators.required]],
    status: [EnquiryStatus.New, [Validators.required]],
    nextFollowUpDate: [''],
  });

  openCreate(): void {
    this.mode.set('create');
    this.initial.set(null);
    this.form.reset({
      fullName: '',
      phone: '',
      email: '',
      courseInterested: '',
      source: EnquirySource.WalkIn,
      status: EnquiryStatus.New,
      nextFollowUpDate: '',
    });
    this.dlg()?.open();
  }

  openEdit(detail: CourseEnquiryDetail): void {
    this.mode.set('edit');
    this.initial.set(detail);
    this.form.reset({
      fullName: detail.fullName,
      phone: detail.phone,
      email: detail.email ?? '',
      courseInterested: detail.courseInterested,
      source: detail.source,
      status: detail.status === EnquiryStatus.Converted ? EnquiryStatus.Interested : detail.status,
      nextFollowUpDate: detail.nextFollowUpDate ?? '',
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    const payload = {
      fullName: v.fullName,
      phone: v.phone,
      email: v.email ? v.email : null,
      courseInterested: v.courseInterested,
      source: v.source,
      status: v.status,
      nextFollowUpDate: v.nextFollowUpDate ? v.nextFollowUpDate : null,
    };

    if (this.isEdit()) {
      const detail = this.initial();
      if (!detail) return;
      this.submitted.emit({ mode: 'edit', enquiryId: detail.enquiryId, payload });
    } else {
      this.submitted.emit({ mode: 'create', payload });
    }
  }
}
