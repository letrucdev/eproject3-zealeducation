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
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { startWith } from 'rxjs';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { ExamResult, OverrideExamResultPayload } from '@core/models/exam-result';
import { Examination } from '@core/models/examination';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';
import { calculateLetterGrade } from '@shared/utils/exam-grade';

export interface ExamResultFormSubmit {
  resultId: string;
  payload: OverrideExamResultPayload;
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
  selector: 'app-exam-result-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmSpinnerImports,
    HlmTextareaImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'exam-result-form-dialog.html',
})
export class ExamResultFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<ExamResultFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  private readonly _examination = signal<Examination | null>(null);
  private readonly _editing = signal<ExamResult | null>(null);

  readonly examination = computed(() => this._examination());
  readonly editingResult = computed(() => this._editing());
  readonly isReOverride = computed(() => this._editing()?.isOverridden === true);

  readonly form = this._fb.nonNullable.group({
    score: this._fb.nonNullable.control<number>(0, [
      Validators.required,
      Validators.min(0),
      scoreWithinMaxValidator(() => this._examination()?.maxScore ?? null),
    ]),
    overrideReason: this._fb.nonNullable.control('', [
      Validators.required,
      Validators.minLength(5),
      Validators.maxLength(500),
    ]),
  });

  private readonly _scoreSignal = toSignal(
    this.form.controls.score.valueChanges.pipe(
      startWith(this.form.controls.score.value),
      takeUntilDestroyed(),
    ),
    { initialValue: this.form.controls.score.value },
  );

  protected readonly derivedGrade = computed(() => {
    const exam = this._examination();
    if (!exam) return '';
    const score = this._scoreSignal();
    return calculateLetterGrade(score == null ? null : Number(score), exam.maxScore, exam.passScore);
  });

  open(examination: Examination, result: ExamResult): void {
    this._examination.set(examination);
    this._editing.set(result);
    this.form.reset({
      score: Number(result.score),
      overrideReason: result.overrideReason ?? '',
    });
    this.form.controls.score.updateValueAndValidity();
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;

    const result = this._editing();
    if (!result) return;

    const v = this.form.getRawValue();
    this.submitted.emit({
      resultId: result.resultId,
      payload: {
        score: v.score,
        overrideReason: v.overrideReason.trim(),
      },
    });
  }
}
