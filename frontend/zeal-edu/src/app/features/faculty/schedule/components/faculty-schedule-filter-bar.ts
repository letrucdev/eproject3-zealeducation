import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { distinctUntilChanged } from 'rxjs';
import { BatchListItem } from '@core/models/batch-list-item';
import { FacultyService } from '../../faculty.service';
import { FacultyCourseOption } from '../../models/faculty-models';

export interface FacultyScheduleFilterValue {
  fromDate: string;
  toDate: string;
  course: FacultyCourseOption | null;
  batch: BatchListItem | null;
}

@Component({
  selector: 'app-faculty-schedule-filter-bar',
  imports: [
    ReactiveFormsModule,
    HlmComboboxImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'faculty-schedule-filter-bar.html',
})
export class FacultyScheduleFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);
  private readonly _service = inject(FacultyService);

  readonly initial = input<FacultyScheduleFilterValue>({
    fromDate: '',
    toDate: '',
    course: null,
    batch: null,
  });
  readonly filterChanged = output<FacultyScheduleFilterValue>();

  protected readonly courseSearch = signal('');
  protected readonly batchSearch = signal('');
  protected readonly selectedCourseId = signal<string | null>(null);

  protected readonly courses = this._service.coursesQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.courseSearch(),
    })),
  );

  protected readonly batches = this._service.batchesListQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.batchSearch(),
      courseId: this.selectedCourseId(),
    })),
  );

  protected readonly courseItemToString = (c: FacultyCourseOption | null): string =>
    c?.courseName ?? '';
  protected readonly batchItemToString = (b: BatchListItem | null): string =>
    b ? `${b.batchCode} (${b.courseName})` : '';

  readonly form = this._fb.group({
    fromDate: this._fb.nonNullable.control(''),
    toDate: this._fb.nonNullable.control(''),
    course: this._fb.control<FacultyCourseOption | null>(null),
    batch: this._fb.control<BatchListItem | null>(null),
  });

  ngOnInit(): void {
    const v = this.initial();
    this.form.patchValue(
      {
        fromDate: v.fromDate,
        toDate: v.toDate,
        course: v.course,
        batch: v.batch,
      },
      { emitEvent: false },
    );
    this.selectedCourseId.set(v.course?.courseId ?? null);

    this.form.controls.fromDate.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        if (!value) return;
        if (value > this.form.controls.toDate.value) {
          this.form.controls.toDate.setValue(value, { emitEvent: false });
        }
        this._emit();
      });

    this.form.controls.toDate.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        if (!value) return;
        if (value < this.form.controls.fromDate.value) {
          this.form.controls.fromDate.setValue(value, { emitEvent: false });
        }
        this._emit();
      });

    this.form.controls.course.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((course) => {
        const nextCourseId = course?.courseId ?? null;
        if (this.selectedCourseId() === nextCourseId) {
          this._emit();
          return;
        }

        this.selectedCourseId.set(nextCourseId);

        // Clear batch nếu course đổi và batch hiện tại không thuộc course mới.
        const currentBatch = this.form.controls.batch.value;
        if (currentBatch && nextCourseId && currentBatch.courseId !== nextCourseId) {
          this.form.controls.batch.setValue(null, { emitEvent: false });
        } else if (currentBatch && !nextCourseId) {
          // Course bị clear: giữ batch (cho phép filter chỉ theo batch).
        }

        this._emit();
      });

    this.form.controls.batch.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    const { fromDate, toDate, course, batch } = this.form.getRawValue();
    if (!fromDate || !toDate) return;
    this.filterChanged.emit({
      fromDate,
      toDate,
      course: course ?? null,
      batch: batch ?? null,
    });
  }
}
