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
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import {
  ClassSession,
  ClassSessionStatus,
  CreateClassSessionPayload,
  UpdateClassSessionPayload,
} from '@core/models/class-session';
import { CLASS_SESSION_STATUS_LABELS } from './session-labels';

export type SessionFormMode = 'create' | 'edit';

export interface SessionFormSubmitCreate {
  mode: 'create';
  batchId: string;
  payload: CreateClassSessionPayload;
}

export interface SessionFormSubmitUpdate {
  mode: 'edit';
  sessionId: string;
  payload: UpdateClassSessionPayload;
}

export type SessionFormSubmit = SessionFormSubmitCreate | SessionFormSubmitUpdate;

const endAfterStartValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const parent = control.parent;
  if (!parent) return null;
  const startTime = parent.get('startTime')?.value as string | null;
  const endTime = control.value as string | null;
  if (!startTime || !endTime) return null;
  return endTime <= startTime ? { endBeforeStart: true } : null;
};

const trimSeconds = (time: string): string => (time?.length > 5 ? time.substring(0, 5) : time ?? '');

@Component({
  selector: 'app-session-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'session-form-dialog.html',
})
export class SessionFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<SessionFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly mode = signal<SessionFormMode>('create');
  private readonly _batchId = signal<string | null>(null);
  private readonly _editingSession = signal<ClassSession | null>(null);

  readonly isEdit = computed(() => this.mode() === 'edit');

  protected readonly statuses = ClassSessionStatus;
  protected readonly statusLabel = (v: ClassSessionStatus): string =>
    CLASS_SESSION_STATUS_LABELS[v];

  readonly form = this._fb.nonNullable.group({
    sessionDate: this._fb.nonNullable.control('', [Validators.required]),
    startTime: this._fb.nonNullable.control('', [Validators.required]),
    endTime: this._fb.nonNullable.control('', [Validators.required, endAfterStartValidator]),
    topic: this._fb.nonNullable.control('', [Validators.maxLength(200)]),
    location: this._fb.nonNullable.control('', [Validators.maxLength(100)]),
    status: this._fb.nonNullable.control<ClassSessionStatus>(ClassSessionStatus.Scheduled),
  });

  constructor() {
    this.form.controls.startTime.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.endTime.updateValueAndValidity());
  }

  openCreate(batchId: string): void {
    this.mode.set('create');
    this._batchId.set(batchId);
    this._editingSession.set(null);
    this.form.reset({
      sessionDate: '',
      startTime: '',
      endTime: '',
      topic: '',
      location: '',
      status: ClassSessionStatus.Scheduled,
    });
    this.dlg()?.open();
  }

  openEdit(session: ClassSession): void {
    this.mode.set('edit');
    this._batchId.set(session.batchId);
    this._editingSession.set(session);
    this.form.reset({
      sessionDate: session.sessionDate,
      startTime: trimSeconds(session.startTime),
      endTime: trimSeconds(session.endTime),
      topic: session.topic ?? '',
      location: session.location ?? '',
      status: session.status,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    const topic = v.topic.trim() || null;
    const location = v.location.trim() || null;
    const startTime = v.startTime.length === 5 ? `${v.startTime}:00` : v.startTime;
    const endTime = v.endTime.length === 5 ? `${v.endTime}:00` : v.endTime;

    if (this.isEdit()) {
      const session = this._editingSession();
      if (!session) return;
      const payload: UpdateClassSessionPayload = {
        sessionDate: v.sessionDate,
        startTime,
        endTime,
        topic,
        location,
        status: v.status,
      };
      this.submitted.emit({ mode: 'edit', sessionId: session.sessionId, payload });
    } else {
      const batchId = this._batchId();
      if (!batchId) return;
      const payload: CreateClassSessionPayload = {
        sessionDate: v.sessionDate,
        startTime,
        endTime,
        topic,
        location,
      };
      this.submitted.emit({ mode: 'create', batchId, payload });
    }
  }
}
