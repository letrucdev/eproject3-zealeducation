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
import { provideIcons } from '@ng-icons/core';
import { lucideSend, lucideUserCheck } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { CourseEnquiryDetail } from '../../../../core/models/course-enquiry-detail';
import { EnquiryStatus } from '../../../../core/models/enquiry-status';
import { ENQUIRY_SOURCE_LABELS, ENQUIRY_STATUS_LABELS } from './enquiry-labels';
import { DatePipe } from '@angular/common';

export interface EnquiryNoteSubmit {
  enquiryId: string;
  content: string;
}

@Component({
  selector: 'app-enquiry-detail-dialog',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    HlmDialogImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmTextareaImports,
    HlmFieldImports,
  ],
  providers: [provideIcons({ lucideSend, lucideUserCheck })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-detail-dialog.html',
})
export class EnquiryDetailDialog {
  private readonly _fb = inject(FormBuilder);

  readonly detail = input<CourseEnquiryDetail | null>(null);
  readonly isAddingNote = input<boolean>(false);

  readonly noteSubmitted = output<EnquiryNoteSubmit>();
  readonly convertClicked = output<CourseEnquiryDetail>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly statuses = EnquiryStatus;

  readonly noteForm = this._fb.nonNullable.group({
    content: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  readonly isOpen = signal(false);

  open(): void {
    this.noteForm.reset({ content: '' });
    this.dlg()?.open();
    this.isOpen.set(true);
  }

  close(): void {
    this.dlg()?.close();
    this.isOpen.set(false);
  }

  submitNote(): void {
    if (this.noteForm.invalid) return;
    const d = this.detail();
    if (!d) return;
    this.noteSubmitted.emit({
      enquiryId: d.enquiryId,
      content: this.noteForm.controls.content.value.trim(),
    });
    this.noteForm.reset({ content: '' });
  }

  resetNoteForm(): void {
    this.noteForm.reset({ content: '' });
  }

  protected statusLabel(value: EnquiryStatus): string {
    return ENQUIRY_STATUS_LABELS[value];
  }

  protected sourceLabel(value: CourseEnquiryDetail['source']): string {
    return ENQUIRY_SOURCE_LABELS[value];
  }

  protected onConvert(): void {
    const d = this.detail();
    if (!d) return;
    this.convertClicked.emit(d);
  }
}
