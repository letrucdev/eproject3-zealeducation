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
import {
  CreateExaminationPayload,
  Examination,
  UpdateExaminationPayload,
} from '@core/models/examination';

export type ExaminationFormMode = 'create' | 'edit';

export interface ExaminationFormSubmitCreate {
  mode: 'create';
  batchId: string;
  payload: CreateExaminationPayload;
}

export interface ExaminationFormSubmitUpdate {
  mode: 'edit';
  examinationId: string;
  payload: UpdateExaminationPayload;
}

export type ExaminationFormSubmit = ExaminationFormSubmitCreate | ExaminationFormSubmitUpdate;

const passWithinMaxValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const parent = control.parent;
  if (!parent) return null;
  const max = parent.get('maxScore')?.value as number | null;
  const pass = control.value as number | null;
  if (max == null || pass == null) return null;
  return pass > max ? { passExceedsMax: true } : null;
};

@Component({
  selector: 'app-examination-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'examination-form-dialog.html',
})
export class ExaminationFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<ExaminationFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly mode = signal<ExaminationFormMode>('create');
  private readonly _batchId = signal<string | null>(null);
  private readonly _editing = signal<Examination | null>(null);

  readonly isEdit = computed(() => this.mode() === 'edit');

  readonly form = this._fb.nonNullable.group({
    examName: this._fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    examDate: this._fb.nonNullable.control('', [Validators.required]),
    location: this._fb.nonNullable.control('', [Validators.maxLength(100)]),
    maxScore: this._fb.nonNullable.control<number>(100, [
      Validators.required,
      Validators.min(1),
    ]),
    passScore: this._fb.nonNullable.control<number>(50, [
      Validators.required,
      Validators.min(0),
      passWithinMaxValidator,
    ]),
  });

  constructor() {
    this.form.controls.maxScore.valueChanges.subscribe(() =>
      this.form.controls.passScore.updateValueAndValidity(),
    );
  }

  openCreate(batchId: string): void {
    this.mode.set('create');
    this._batchId.set(batchId);
    this._editing.set(null);
    this.form.reset({
      examName: '',
      examDate: '',
      location: '',
      maxScore: 100,
      passScore: 50,
    });
    this.dlg()?.open();
  }

  openEdit(examination: Examination): void {
    this.mode.set('edit');
    this._batchId.set(examination.batchId);
    this._editing.set(examination);
    this.form.reset({
      examName: examination.examName,
      examDate: examination.examDate,
      location: examination.location ?? '',
      maxScore: examination.maxScore,
      passScore: examination.passScore,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    const payload: CreateExaminationPayload = {
      examName: v.examName.trim(),
      examDate: v.examDate,
      location: v.location.trim() || null,
      maxScore: v.maxScore,
      passScore: v.passScore,
    };

    if (this.isEdit()) {
      const exam = this._editing();
      if (!exam) return;
      this.submitted.emit({ mode: 'edit', examinationId: exam.examinationId, payload });
    } else {
      const batchId = this._batchId();
      if (!batchId) return;
      this.submitted.emit({ mode: 'create', batchId, payload });
    }
  }
}
