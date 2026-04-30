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
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';
import { calculateLetterGrade } from '@shared/utils/exam-grade';
import {
  FacultyExaminationCandidate,
  FacultyExaminationSummary,
} from '../../models/faculty-models';

export interface FacultyExamScoreSubmit {
  examinationId: string;
  candidate: FacultyExaminationCandidate;
  score: number;
  isFinalized: boolean;
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
    ConfirmDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'faculty-exam-score-form-dialog.html',
})
export class FacultyExamScoreFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<FacultyExamScoreSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly finalizeConfirmDialog = viewChild<ConfirmDialog>('finalizeConfirmDialog');

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
    return calculateLetterGrade(
      score == null ? null : Number(score),
      exam.maxScore,
      exam.passScore,
    );
  });

  open(examination: FacultyExaminationSummary, candidate: FacultyExaminationCandidate): void {
    this._examination.set(examination);
    this._candidate.set(candidate);
    this.form.reset({
      score: candidate.score != null ? Number(candidate.score) : 0,
    });
    this.form.controls.score.updateValueAndValidity();
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected requestFinalize(): void {
    if (this.form.invalid || this.submitting()) return;
    const candidate = this._candidate();
    if (!candidate) return;
    const v = this.form.getRawValue();
    const grade = this.derivedGrade() || '–';
    this.finalizeConfirmDialog()?.open({
      title: 'Finalize this score?',
      message:
        `Candidate: ${candidate.candidateCode} — ${candidate.candidateFullName}\n` +
        `Score: ${v.score} · Grade: ${grade}\n\n` +
        `This score will be locked for the candidate above. ` +
        `You won't be able to edit it later — only an Incharge can override.`,
      confirmLabel: 'Confirm finalize',
      cancelLabel: 'Back',
    });
  }

  protected onSubmit(isFinalized: boolean): void {
    if (this.form.invalid || this.submitting()) return;

    const examination = this._examination();
    const candidate = this._candidate();
    if (!examination || !candidate) return;

    const v = this.form.getRawValue();
    this.submitted.emit({
      examinationId: examination.examinationId,
      candidate,
      score: v.score,
      isFinalized,
    });
  }
}
