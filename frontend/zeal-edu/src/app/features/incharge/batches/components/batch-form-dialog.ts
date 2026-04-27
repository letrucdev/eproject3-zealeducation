import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
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
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { BatchDetail } from '@core/models/batch-detail';
import { BatchStatus } from '@core/models/batch-status';
import { CourseListItem } from '@core/models/course-list-item';
import { FacultyListItem } from '@core/models/faculty-list-item';
import { CoursesService } from '@core/services/courses.service';
import { FacultiesService } from '@core/services/faculties.service';
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';
import { CreateBatchPayload, UpdateBatchPayload } from '../models/batch-payload';
import { BATCH_STATUS_LABELS } from './batch-labels';

export type BatchFormMode = 'create' | 'edit';

export interface BatchFormSubmitCreate {
  mode: 'create';
  payload: CreateBatchPayload;
}

export interface BatchFormSubmitUpdate {
  mode: 'edit';
  batchId: string;
  payload: UpdateBatchPayload;
}

export type BatchFormSubmit = BatchFormSubmitCreate | BatchFormSubmitUpdate;

const endAfterStartValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const parent = control.parent;
  if (!parent) return null;
  const startDate = parent.get('startDate')?.value as string | null;
  const endDate = control.value as string | null;
  if (!startDate || !endDate) return null;
  return new Date(endDate) <= new Date(startDate) ? { endBeforeStart: true } : null;
};

const todayIso = (): string => {
  const now = new Date();
  const yyyy = now.getFullYear();
  const mm = String(now.getMonth() + 1).padStart(2, '0');
  const dd = String(now.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
};

const startNotPastValidator =
  (initial: () => BatchDetail | null): ValidatorFn =>
  (control) => {
    const value = control.value as string | null;
    if (!value) return null;
    const original = initial()?.startDate ?? null;
    if (original && value === original) return null;
    return value < todayIso() ? { startInPast: true } : null;
  };

@Component({
  selector: 'app-batch-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSelectImports,
    HlmComboboxImports,
    HlmSpinnerImports,
    HlmButtonImports,
    ConfirmDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-form-dialog.html',
})
export class BatchFormDialog {
  private readonly _fb = inject(FormBuilder);
  private readonly _coursesService = inject(CoursesService);
  private readonly _facultiesService = inject(FacultiesService);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<BatchFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly confirmDialog = viewChild.required<ConfirmDialog>('confirmDialog');

  private readonly _pendingSubmit = signal<BatchFormSubmitUpdate | null>(null);

  readonly mode = signal<BatchFormMode>('create');
  readonly initial = signal<BatchDetail | null>(null);
  readonly isEdit = computed(() => this.mode() === 'edit');

  protected readonly startDateMin = computed(() => {
    const today = todayIso();
    const original = this.initial()?.startDate;
    if (original && original < today) return original;
    return today;
  });

  protected readonly statuses = BatchStatus;
  protected readonly statusLabel = (v: BatchStatus): string => BATCH_STATUS_LABELS[v];

  readonly form = this._fb.group({
    batchCode: this._fb.nonNullable.control('', [Validators.required, Validators.maxLength(30)]),
    course: this._fb.control<CourseListItem | null>(null, [Validators.required]),
    faculty: this._fb.control<FacultyListItem | null>(null),
    startDate: this._fb.nonNullable.control('', [
      Validators.required,
      startNotPastValidator(() => this.initial()),
    ]),
    endDate: this._fb.nonNullable.control('', [Validators.required, endAfterStartValidator]),
    location: this._fb.nonNullable.control('', [Validators.maxLength(100)]),
    maxCapacity: this._fb.nonNullable.control<number>(30, [
      Validators.required,
      Validators.min(1),
      Validators.max(500),
    ]),
    status: this._fb.nonNullable.control<BatchStatus>(BatchStatus.NeedsInstructor),
  });

  private readonly currentStatus = toSignal(this.form.controls.status.valueChanges, {
    initialValue: this.form.controls.status.value,
  });

  protected readonly courseSearch = signal('');
  protected readonly facultySearch = signal('');

  constructor() {
    this.form.controls.startDate.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.endDate.updateValueAndValidity());

    effect(() => {
      const control = this.form.controls.status;
      const shouldDisable = this.currentStatus() === BatchStatus.NeedsInstructor;
      if (!shouldDisable) {
        return control.enable({ emitEvent: false });
      }

      control.disable({ emitEvent: false });
    });
  }

  protected readonly courses = this._coursesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.courseSearch(),
      isActive: true,
    })),
  );

  protected readonly faculties = this._facultiesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.facultySearch(),
    })),
  );

  protected readonly courseItemToString = (c: CourseListItem | null): string => c?.courseName ?? '';

  protected readonly facultyItemToString = (f: FacultyListItem | null): string =>
    f ? `${f.fullName} (${f.facultyCode})` : '';

  openCreate(): void {
    this.mode.set('create');
    this.initial.set(null);
    this.courseSearch.set('');
    this.facultySearch.set('');
    this.form.reset({
      batchCode: '',
      course: null,
      faculty: null,
      startDate: '',
      endDate: '',
      location: '',
      maxCapacity: 30,
      status: BatchStatus.NeedsInstructor,
    });
    this.dlg()?.open();
  }

  openEdit(detail: BatchDetail): void {
    this.mode.set('edit');
    this.initial.set(detail);
    this.courseSearch.set('');
    this.facultySearch.set('');
    this.form.reset({
      batchCode: detail.batchCode,
      course: {
        courseId: detail.courseId,
        courseName: detail.courseName,
        description: null,
        durationWeeks: 0,
        baseFee: 0,
        isActive: true,
        createdAt: '',
        updatedAt: null,
      },
      faculty: null,
      startDate: detail.startDate,
      endDate: detail.endDate,
      location: detail.location ?? '',
      maxCapacity: detail.maxCapacity,
      status: detail.status,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    if (!v.course) return;

    const location = v.location.trim() ? v.location.trim() : null;
    const batchCode = v.batchCode.trim();

    if (this.isEdit()) {
      const detail = this.initial();
      if (!detail) return;
      const payload: UpdateBatchPayload = {
        batchCode,
        courseId: v.course.courseId,
        startDate: v.startDate,
        endDate: v.endDate,
        location,
        maxCapacity: Number(v.maxCapacity),
        status: v.status,
      };

      const isTerminal =
        v.status === BatchStatus.Completed || v.status === BatchStatus.Cancelled;
      const wasTerminal =
        detail.status === BatchStatus.Completed || detail.status === BatchStatus.Cancelled;
      if (isTerminal && !wasTerminal) {
        this._pendingSubmit.set({ mode: 'edit', batchId: detail.batchId, payload });
        this.confirmDialog().open({
          title:
            v.status === BatchStatus.Completed ? 'Mark batch as completed' : 'Cancel batch',
          message: 'This action cannot be undone. Continue?',
          confirmLabel: 'Confirm',
          destructive: v.status === BatchStatus.Cancelled,
        });
        return;
      }

      this.submitted.emit({ mode: 'edit', batchId: detail.batchId, payload });
    } else {
      const payload: CreateBatchPayload = {
        batchCode,
        courseId: v.course.courseId,
        facultyId: v.faculty?.facultyId ?? null,
        startDate: v.startDate,
        endDate: v.endDate,
        location,
        maxCapacity: Number(v.maxCapacity),
      };
      this.submitted.emit({ mode: 'create', payload });
    }
  }

  protected onStatusConfirmed(): void {
    const pending = this._pendingSubmit();
    if (!pending) return;
    this._pendingSubmit.set(null);
    this.submitted.emit(pending);
  }

  protected onStatusCancelled(): void {
    this._pendingSubmit.set(null);
  }
}
