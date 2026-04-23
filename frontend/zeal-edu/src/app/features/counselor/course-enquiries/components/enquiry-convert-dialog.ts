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
import { provideIcons } from '@ng-icons/core';
import { lucideCheckCheck, lucideCopy, lucideUserCheck } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { ConvertEnquiryResult } from '@core/models/convert-enquiry-result';
import { CourseEnquiryDetail } from '@core/models/course-enquiry-detail';
import { Gender } from '@core/models/gender';
import { ConvertEnquiryPayload } from '@features/counselor/course-enquiries/models/course-enquiry-payload';

export interface EnquiryConvertSubmit {
  enquiryId: string;
  payload: ConvertEnquiryPayload;
}

@Component({
  selector: 'app-enquiry-convert-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    HlmTextareaImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideUserCheck, lucideCheckCheck, lucideCopy })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-convert-dialog.html',
})
export class EnquiryConvertDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<EnquiryConvertSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly genders = Gender;

  protected readonly genderLabel = (value: Gender): string => {
    switch (value) {
      case Gender.Male:
        return 'Male';
      case Gender.Female:
        return 'Female';
      case Gender.Other:
        return 'Other';
      default:
        return '';
    }
  };

  readonly enquiry = signal<CourseEnquiryDetail | null>(null);
  readonly result = signal<ConvertEnquiryResult | null>(null);
  readonly copyState = signal<'idle' | 'copied'>('idle');

  readonly showResult = computed(() => this.result() !== null);

  readonly form = this._fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    dob: ['', [Validators.required]],
    gender: [Gender.Male, [Validators.required]],
    address: [''],
    emergencyContact: [''],
  });

  open(detail: CourseEnquiryDetail): void {
    this.enquiry.set(detail);
    this.result.set(null);
    this.copyState.set('idle');
    this.form.reset({
      email: detail.email ?? '',
      dob: '',
      gender: Gender.Male,
      address: '',
      emergencyContact: '',
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  showCredentials(result: ConvertEnquiryResult): void {
    this.result.set(result);
  }

  submit(): void {
    if (this.form.invalid) return;
    const detail = this.enquiry();
    if (!detail) return;
    const v = this.form.getRawValue();
    this.submitted.emit({
      enquiryId: detail.enquiryId,
      payload: {
        email: v.email,
        dob: v.dob,
        gender: v.gender,
        address: v.address ? v.address : null,
        emergencyContact: v.emergencyContact ? v.emergencyContact : null,
      },
    });
  }

  async copyCredentials(): Promise<void> {
    const r = this.result();
    if (!r) return;
    const text = `Username: ${r.username}\nTemporary password: ${r.temporaryPassword}\nEmail: ${r.email}\nCandidate code: ${r.candidateCode}`;
    try {
      await navigator.clipboard.writeText(text);
      this.copyState.set('copied');
      setTimeout(() => this.copyState.set('idle'), 2000);
    } catch {
      this.copyState.set('idle');
    }
  }
}
