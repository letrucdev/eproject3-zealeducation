import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCheckboxImports } from '@spartan-ng/helm/checkbox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { BatchDetail } from '@core/models/batch-detail';
import { CreateBulkSessionsPayload } from '../models/batch-payload';

export interface BulkSessionFormSubmit {
  batchId: string;
  payload: CreateBulkSessionsPayload;
}

interface DayOption {
  value: number; // System.DayOfWeek (Sun=0..Sat=6)
  label: string;
}

const DAY_OPTIONS: ReadonlyArray<DayOption> = [
  { value: 1, label: 'T2' },
  { value: 2, label: 'T3' },
  { value: 3, label: 'T4' },
  { value: 4, label: 'T5' },
  { value: 5, label: 'T6' },
  { value: 6, label: 'T7' },
  { value: 0, label: 'CN' },
];

const endAfterStartValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const parent = control.parent;
  if (!parent) return null;
  const startTime = parent.get('startTime')?.value as string | null;
  const endTime = control.value as string | null;
  if (!startTime || !endTime) return null;
  return endTime <= startTime ? { endBeforeStart: true } : null;
};

const minOneDayValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as number[] | null;
  return value && value.length > 0 ? null : { required: true };
};

@Component({
  selector: 'app-bulk-session-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmCheckboxImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'bulk-session-form-dialog.html',
})
export class BulkSessionFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<BulkSessionFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly dayOptions = DAY_OPTIONS;

  private readonly _batch = signal<BatchDetail | null>(null);
  protected readonly batch = this._batch.asReadonly();

  readonly form = this._fb.nonNullable.group({
    daysOfWeek: this._fb.nonNullable.control<number[]>([], [minOneDayValidator]),
    startTime: this._fb.nonNullable.control('', [Validators.required]),
    endTime: this._fb.nonNullable.control('', [Validators.required, endAfterStartValidator]),
    topic: this._fb.nonNullable.control('', [Validators.maxLength(200)]),
    location: this._fb.nonNullable.control('', [Validators.maxLength(100)]),
  });

  constructor() {
    this.form.controls.startTime.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.endTime.updateValueAndValidity());
  }

  open(batch: BatchDetail): void {
    this._batch.set(batch);
    this.form.reset({
      daysOfWeek: [],
      startTime: '',
      endTime: '',
      topic: '',
      location: '',
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected isDaySelected(value: number): boolean {
    return this.form.controls.daysOfWeek.value.includes(value);
  }

  protected toggleDay(value: number, checked: boolean): void {
    const ctrl = this.form.controls.daysOfWeek;
    const current = new Set(ctrl.value);
    if (checked) {
      current.add(value);
    } else {
      current.delete(value);
    }
    ctrl.setValue([...current].sort((a, b) => a - b));
    ctrl.markAsDirty();
  }

  protected setPreset(values: number[]): void {
    const ctrl = this.form.controls.daysOfWeek;
    ctrl.setValue([...values].sort((a, b) => a - b));
    ctrl.markAsDirty();
  }

  protected presetWholeWeek(): void {
    this.setPreset([0, 1, 2, 3, 4, 5, 6]);
  }

  protected presetMwf(): void {
    this.setPreset([1, 3, 5]);
  }

  protected presetTt(): void {
    this.setPreset([2, 4]);
  }

  protected presetWeekend(): void {
    this.setPreset([0, 6]);
  }

  submit(): void {
    if (this.form.invalid) return;
    const batch = this._batch();
    if (!batch) return;

    const v = this.form.getRawValue();
    const startTime = v.startTime.length === 5 ? `${v.startTime}:00` : v.startTime;
    const endTime = v.endTime.length === 5 ? `${v.endTime}:00` : v.endTime;
    const topic = v.topic.trim() || null;
    const location = v.location.trim() || null;

    const payload: CreateBulkSessionsPayload = {
      daysOfWeek: v.daysOfWeek,
      startTime,
      endTime,
      topic,
      location,
    };

    this.submitted.emit({ batchId: batch.batchId, payload });
  }
}
