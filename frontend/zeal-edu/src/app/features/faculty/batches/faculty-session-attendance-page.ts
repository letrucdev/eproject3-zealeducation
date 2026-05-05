import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { provideIcons } from '@ng-icons/core';
import { lucideArrowLeft, lucideSave } from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { AttendanceStatus, MarkAttendanceEntry } from '@core/models/attendance';
import { ClassSessionStatus } from '@core/models/class-session';
import { ATTENDANCE_STATUS_LABELS } from '@core/models/session-labels';
import { FacultyService } from '../faculty.service';

interface AttendanceFormRow {
  status: AttendanceStatus;
  practicalHours: string;
  remarks: string;
}

function computeDurationHours(startTime: string, endTime: string): number | null {
  const parse = (value: string): number | null => {
    const [h, m] = value.split(':');
    const hours = Number(h);
    const minutes = Number(m);
    if (!Number.isFinite(hours) || !Number.isFinite(minutes)) return null;
    return hours + minutes / 60;
  };
  const start = parse(startTime);
  const end = parse(endTime);
  if (start == null || end == null) return null;
  const diff = end - start;
  if (diff <= 0) return null;
  return Math.round(diff * 100) / 100;
}

function isPracticalHoursValid(raw: string, max: number): boolean {
  const trimmed = raw.trim();
  if (trimmed === '') return true;
  const value = Number(trimmed);
  if (!Number.isFinite(value)) return false;
  if (value < 0) return false;
  return value <= max;
}

@Component({
  selector: 'app-faculty-session-attendance-page',
  imports: [
    DatePipe,
    RouterLink,
    HlmBadgeImports,
    HlmButtonImports,
    HlmCardImports,
    HlmFieldImports,
    HlmIconImports,
    HlmInputImports,
    HlmSelectImports,
    HlmSkeletonImports,
  ],
  providers: [provideIcons({ lucideArrowLeft, lucideSave })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'faculty-session-attendance-page.html',
})
export default class FacultySessionAttendancePage {
  private readonly _route = inject(ActivatedRoute);
  private readonly _service = inject(FacultyService);

  private readonly _routeParam = toSignal(this._route.paramMap, { initialValue: null });

  protected readonly currentBatchId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('batchId') ?? null;
  });

  protected readonly currentSessionId = computed<string | null>(() => {
    const map = this._routeParam();
    return map?.get('sessionId') ?? null;
  });

  protected readonly attendanceQuery = this._service.sessionAttendanceQuery(this.currentSessionId);
  protected readonly markMutation = this._service.markAttendanceMutation();

  protected readonly statuses = AttendanceStatus;
  protected readonly sessionStatuses = ClassSessionStatus;
  protected readonly statusLabel = (v: AttendanceStatus): string => ATTENDANCE_STATUS_LABELS[v];

  private readonly _formState = signal<Map<string, AttendanceFormRow>>(new Map());

  protected readonly backLink = computed(() => {
    const id = this.currentBatchId();
    return id ? `/app/faculty/batches/${id}` : '/app/faculty/batches';
  });

  protected readonly isLocked = computed(() => {
    const data = this.attendanceQuery.data();
    return data?.status === ClassSessionStatus.Cancelled;
  });

  protected readonly maxPracticalHours = computed<number | null>(() => {
    const data = this.attendanceQuery.data();
    if (!data) return null;
    return computeDurationHours(data.startTime, data.endTime);
  });

  protected readonly hasInvalidPracticalHours = computed(() => {
    const max = this.maxPracticalHours();
    if (max == null) return false;
    for (const row of this._formState().values()) {
      if (!isPracticalHoursValid(row.practicalHours, max)) return true;
    }
    return false;
  });

  constructor() {
    effect(() => {
      const data = this.attendanceQuery.data();
      if (!data) return;
      untracked(() => {
        const next = new Map<string, AttendanceFormRow>();
        for (const row of data.rows) {
          next.set(row.enrollmentId, {
            status: row.status ?? AttendanceStatus.Present,
            practicalHours: row.practicalHours != null ? String(row.practicalHours) : '',
            remarks: row.remarks ?? '',
          });
        }
        this._formState.set(next);
      });
    });
  }

  protected getStatus(enrollmentId: string): AttendanceStatus {
    return this._formState().get(enrollmentId)?.status ?? AttendanceStatus.Present;
  }

  protected getRemarks(enrollmentId: string): string {
    return this._formState().get(enrollmentId)?.remarks ?? '';
  }

  protected getPracticalHours(enrollmentId: string): string {
    return this._formState().get(enrollmentId)?.practicalHours ?? '';
  }

  protected isPracticalHoursRowInvalid(enrollmentId: string): boolean {
    const max = this.maxPracticalHours();
    if (max == null) return false;
    const raw = this._formState().get(enrollmentId)?.practicalHours ?? '';
    return !isPracticalHoursValid(raw, max);
  }

  protected onStatusChanged(enrollmentId: string, status: AttendanceStatus | null): void {
    if (!status) return;
    this._formState.update((prev) => {
      const next = new Map(prev);
      const current = next.get(enrollmentId) ?? { status, practicalHours: '', remarks: '' };
      next.set(enrollmentId, { ...current, status });
      return next;
    });
  }

  protected onRemarksChanged(enrollmentId: string, remarks: string): void {
    this._formState.update((prev) => {
      const next = new Map(prev);
      const current = next.get(enrollmentId) ?? {
        status: AttendanceStatus.Present,
        practicalHours: '',
        remarks,
      };
      next.set(enrollmentId, { ...current, remarks });
      return next;
    });
  }

  protected onPracticalHoursChanged(enrollmentId: string, practicalHours: string): void {
    this._formState.update((prev) => {
      const next = new Map(prev);
      const current = next.get(enrollmentId) ?? {
        status: AttendanceStatus.Present,
        practicalHours,
        remarks: '',
      };
      next.set(enrollmentId, { ...current, practicalHours });
      return next;
    });
  }

  protected onSave(): void {
    const sessionId = this.currentSessionId();
    if (!sessionId) return;
    const data = this.attendanceQuery.data();
    if (!data) return;

    const max = this.maxPracticalHours();
    if (max != null && this.hasInvalidPracticalHours()) {
      toast.error(`Practical hours cannot exceed the session duration (${max} hours).`);
      return;
    }

    const entries: MarkAttendanceEntry[] = data.rows.map((row) => {
      const formRow = this._formState().get(row.enrollmentId);
      const hoursRaw = formRow?.practicalHours?.trim() ?? '';
      const hoursParsed = hoursRaw === '' ? Number.NaN : Number(hoursRaw);
      return {
        enrollmentId: row.enrollmentId,
        status: formRow?.status ?? AttendanceStatus.Present,
        practicalHours: Number.isFinite(hoursParsed) ? hoursParsed : null,
        remarks: formRow?.remarks?.trim() ? formRow.remarks.trim() : null,
      };
    });

    this.markMutation.mutate(
      { sessionId, payload: { entries } },
      {
        onSuccess: () => toast.success('Attendance saved.'),
      },
    );
  }
}
