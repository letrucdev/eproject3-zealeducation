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
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { SubmitCourseFeedbackPayload } from '../../models/feedback-payload';
import { MyBatchDetail } from '../../models/candidate-portal-models';
import { RatingInput } from './rating-input';

@Component({
  selector: 'app-course-feedback-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmTextareaImports,
    HlmButtonImports,
    HlmSpinnerImports,
    RatingInput,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-md w-md"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          <h2 hlmDialogTitle>Course feedback</h2>
          @if (batch(); as b) {
            <p class="text-muted-foreground text-sm">
              Rate the curriculum and content of {{ b.courseName }}.
            </p>
          }
        </div>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="mt-2 flex flex-col gap-4">
          <hlm-field-group>
            <hlm-field>
              <label hlmFieldLabel for="course-feedback-rating">Rating</label>
              <app-rating-input
                id="course-feedback-rating"
                formControlName="rating"
              ></app-rating-input>
              <hlm-field-error validator="required">Rating is required.</hlm-field-error>
              <hlm-field-error validator="min">Please give a rating from 1 to 5.</hlm-field-error>
            </hlm-field>

            <hlm-field>
              <label hlmFieldLabel for="course-feedback-comment">Comment (optional)</label>
              <textarea
                hlmTextarea
                id="course-feedback-comment"
                formControlName="comment"
                rows="4"
                maxlength="1000"
                placeholder="What did you think of the curriculum?"
              ></textarea>
              <hlm-field-error validator="maxlength">Comment must be 1000 characters or fewer.</hlm-field-error>
            </hlm-field>
          </hlm-field-group>

          <div hlmDialogFooter>
            <button hlmBtn variant="outline" type="button" hlmDialogClose>Cancel</button>
            <button hlmBtn type="submit" [disabled]="form.invalid || submitting()">
              @if (submitting()) {
                <hlm-spinner class="mr-2" />
                Submitting...
              } @else {
                Submit feedback
              }
            </button>
          </div>
        </form>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class CourseFeedbackDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<SubmitCourseFeedbackPayload>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly batch = signal<MyBatchDetail | null>(null);

  readonly form = this._fb.nonNullable.group({
    rating: this._fb.nonNullable.control(0, [Validators.required, Validators.min(1)]),
    comment: this._fb.nonNullable.control('', [Validators.maxLength(1000)]),
  });

  open(batch: MyBatchDetail): void {
    this.batch.set(batch);
    this.form.reset({ rating: 0, comment: '' });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.submitting()) return;
    const batch = this.batch();
    if (!batch) return;
    const v = this.form.getRawValue();
    this.submitted.emit({
      batchId: batch.batchId,
      rating: v.rating,
      comment: v.comment.trim() || null,
    });
  }
}
