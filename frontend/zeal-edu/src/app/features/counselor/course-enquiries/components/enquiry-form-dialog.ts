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
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { DigitsOnlyDirective } from '@shared/directives/digits-only.directive';
import { CourseEnquiryDetail } from '@core/models/course-enquiry-detail';
import { CourseListItem } from '@core/models/course-list-item';
import { EnquirySource } from '@core/models/enquiry-source';
import { EnquiryStatus } from '@core/models/enquiry-status';
import { CoursesService } from '@core/services/courses.service';
import {
  CreateEnquiryPayload,
  UpdateEnquiryPayload,
} from '@features/counselor/course-enquiries/models/course-enquiry-payload';
import { ENQUIRY_SOURCE_LABELS, ENQUIRY_STATUS_LABELS } from './enquiry-labels';
import { VndPipe } from '@shared/pipes/vnd-pipe';

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
    HlmComboboxImports,
    HlmSpinnerImports,
    HlmButtonImports,
    DigitsOnlyDirective,
    VndPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-form-dialog.html',
})
export class EnquiryFormDialog {
  private readonly _fb = inject(FormBuilder);
  private readonly _coursesService = inject(CoursesService);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<EnquiryFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly sources = EnquirySource;
  protected readonly statuses = EnquiryStatus;

  protected readonly sourceLabel = (value: EnquirySource): string => ENQUIRY_SOURCE_LABELS[value];

  protected readonly statusLabel = (value: EnquiryStatus): string => ENQUIRY_STATUS_LABELS[value];

  readonly mode = signal<EnquiryFormMode>('create');
  readonly initial = signal<CourseEnquiryDetail | null>(null);

  readonly isEdit = computed(() => this.mode() === 'edit');

  private readonly _takenPhones = signal<ReadonlySet<string>>(new Set());
  private readonly _takenEmails = signal<ReadonlySet<string>>(new Set());

  private readonly _phoneTakenValidator: ValidatorFn = (control: AbstractControl) => {
    const value = (control.value as string | null)?.trim();
    if (!value) return null;
    return this._takenPhones().has(value) ? { phoneTaken: true } : null;
  };

  private readonly _emailTakenValidator: ValidatorFn = (control: AbstractControl) => {
    const value = (control.value as string | null)?.trim().toLowerCase();
    if (!value) return null;
    return this._takenEmails().has(value) ? { emailTaken: true } : null;
  };

  readonly form = this._fb.group({
    fullName: this._fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    phone: this._fb.nonNullable.control('', [
      Validators.required,
      Validators.minLength(10),
      Validators.maxLength(10),
      Validators.pattern(/^\d+$/),
      this._phoneTakenValidator,
    ]),
    email: this._fb.nonNullable.control('', [
      Validators.email,
      Validators.maxLength(100),
      this._emailTakenValidator,
    ]),
    courseInterested: this._fb.control<CourseListItem | null>(null, [Validators.required]),
    source: this._fb.nonNullable.control(EnquirySource.WalkIn, [Validators.required]),
    status: this._fb.nonNullable.control(EnquiryStatus.New, [Validators.required]),
    nextFollowUpDate: this._fb.nonNullable.control(''),
  });

  protected readonly courseSearch = signal('');

  protected readonly courses = this._coursesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 10,
      search: this.courseSearch(),
      isActive: true,
    })),
  );

  protected readonly courseItemToString = (course: CourseListItem | null): string =>
    course?.courseName ?? '';

  markPhoneTaken(phone: string): void {
    const trimmed = phone.trim();
    if (!trimmed) return;
    const next = new Set(this._takenPhones());
    next.add(trimmed);
    this._takenPhones.set(next);
    this.form.controls.phone.updateValueAndValidity();
    this.form.controls.phone.markAsTouched();
  }

  markEmailTaken(email: string): void {
    const trimmed = email.trim().toLowerCase();
    if (!trimmed) return;
    const next = new Set(this._takenEmails());
    next.add(trimmed);
    this._takenEmails.set(next);
    this.form.controls.email.updateValueAndValidity();
    this.form.controls.email.markAsTouched();
  }

  openCreate(): void {
    this.mode.set('create');
    this.initial.set(null);
    this.courseSearch.set('');
    this._takenPhones.set(new Set());
    this._takenEmails.set(new Set());
    this.form.reset({
      fullName: '',
      phone: '',
      email: '',
      courseInterested: null,
      source: EnquirySource.WalkIn,
      status: EnquiryStatus.New,
      nextFollowUpDate: '',
    });
    this.dlg()?.open();
  }

  openEdit(detail: CourseEnquiryDetail): void {
    this.mode.set('edit');
    this.initial.set(detail);
    this.courseSearch.set('');
    this._takenPhones.set(new Set());
    this._takenEmails.set(new Set());
    this.form.reset({
      fullName: detail.fullName,
      phone: detail.phone,
      email: detail.email ?? '',
      courseInterested: {
        courseId: detail.courseInterestedId,
        courseName: detail.courseInterestedName,
        description: null,
        durationWeeks: 0,
        baseFee: 0,
        isActive: true,
        createdAt: '',
        updatedAt: null,
      },
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
    if (!v.courseInterested) return;

    const payload = {
      fullName: v.fullName,
      phone: v.phone,
      email: v.email ? v.email : null,
      courseInterestedId: v.courseInterested.courseId,
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
