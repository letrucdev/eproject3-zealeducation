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
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { FacultyExaminationCandidate, FacultyExaminationSummary } from '../../models/faculty-models';

export interface FacultyExamScoreSubmit {
  examinationId: string;
  candidate: FacultyExaminationCandidate;
  score: number;
  grade: string | null;
}

const scoreWithinMaxValidator =
  (maxScore: () => number | null): ValidatorFn =>
  (control: AbstractControl): ValidationErrors | null => {
    const max = maxScore();
    const value = control.value as number | null;
    if (max == null || value == null) return null;
    return value > max ? { scoreExceedsMax: true } : null;
  };

@Component({
  selector: 'app-faculty-exam-score-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-lg w-lg flex max-h-[90dvh] flex-col"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          <h2 hlmDialogTitle>
            {{ isEdit() ? 'Update score' : 'Enter score' }}
          </h2>
          @if (examination(); as exam) {
            <p class="text-muted-foreground text-sm">
              {{ exam.examName }} — Max {{ exam.maxScore }}, Pass {{ exam.passScore }}.
            </p>
          }
        </div>

        <form
          [formGroup]="form"
          (ngSubmit)="onSubmit()"
          class="mt-2 flex min-h-0 flex-1 flex-col gap-4"
        >
          <hlm-field-group class="min-h-0 flex-1 overflow-y-auto overflow-x-hidden">
            @if (candidate(); as c) {
              <div class="bg-muted/50 rounded-md border p-3 text-sm">
                <div class="text-muted-foreground">Candidate</div>
                <div class="font-medium">{{ c.candidateCode }} — {{ c.candidateFullName }}</div>
              </div>
            }

            <hlm-field>
              <label hlmFieldLabel for="faculty-exam-score">Score</label>
              <input
                hlmInput
                id="faculty-exam-score"
                type="number"
                formControlName="score"
                class="w-full"
                min="0"
                [attr.max]="examination()?.maxScore ?? null"
                step="0.01"
              />
              <hlm-field-error validator="required">Score is required.</hlm-field-error>
              <hlm-field-error validator="min">Score cannot be negative.</hlm-field-error>
              <hlm-field-error validator="scoreExceedsMax">
                Score cannot exceed the exam max score.
              </hlm-field-error>
            </hlm-field>

            <hlm-field>
              <label hlmFieldLabel for="faculty-exam-grade">Grade (optional)</label>
              <input
                hlmInput
                id="faculty-exam-grade"
                type="text"
                formControlName="grade"
                class="w-full"
                maxlength="5"
                placeholder="e.g. A, B+, P"
              />
              <hlm-field-error validator="maxlength">Max 5 characters.</hlm-field-error>
            </hlm-field>
          </hlm-field-group>

          <div hlmDialogFooter class="shrink-0">
            <button hlmBtn variant="outline" type="button" hlmDialogClose>Cancel</button>
            <button hlmBtn type="submit" [disabled]="form.invalid || submitting()">
              @if (submitting()) {
                <hlm-spinner class="mr-2" />
                Saving...
              } @else if (isEdit()) {
                Save changes
              } @else {
                Save score
              }
            </button>
          </div>
        </form>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class FacultyExamScoreFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<FacultyExamScoreSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  private readonly _examination = signal<FacultyExaminationSummary | null>(null);
  private readonly _candidate = signal<FacultyExaminationCandidate | null>(null);

  readonly examination = computed(() => this._examination());
  readonly candidate = computed(() => this._candidate());
  readonly isEdit = computed(() => this._candidate()?.resultId != null);

  readonly form = this._fb.nonNullable.group({
    score: this._fb.nonNullable.control<number>(0, [
      Validators.required,
      Validators.min(0),
      scoreWithinMaxValidator(() => this._examination()?.maxScore ?? null),
    ]),
    grade: this._fb.nonNullable.control('', [Validators.maxLength(5)]),
  });

  open(examination: FacultyExaminationSummary, candidate: FacultyExaminationCandidate): void {
    this._examination.set(examination);
    this._candidate.set(candidate);
    this.form.reset({
      score: candidate.score != null ? Number(candidate.score) : 0,
      grade: candidate.grade ?? '',
    });
    this.form.controls.score.updateValueAndValidity();
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.submitting()) return;

    const examination = this._examination();
    const candidate = this._candidate();
    if (!examination || !candidate) return;

    const v = this.form.getRawValue();
    this.submitted.emit({
      examinationId: examination.examinationId,
      candidate,
      score: v.score,
      grade: v.grade.trim() || null,
    });
  }
}
