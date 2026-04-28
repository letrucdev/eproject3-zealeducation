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
import { provideIcons } from '@ng-icons/core';
import { lucideSearch } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CandidateStatus } from '@core/models/candidate-status';
import { BatchListItem } from '@core/models/batch-list-item';
import { CourseListItem } from '@core/models/course-list-item';
import { CoursesService } from '@core/services/courses.service';
import { BatchesService } from '@features/incharge/batches/batches.service';
import { CANDIDATE_STATUS_LABELS } from '../models/candidate-labels';

type StatusFilterValue = CandidateStatus | 'all';

export interface CandidateFilterValue {
  search: string;
  status: CandidateStatus | null;
  course: CourseListItem | null;
  batch: BatchListItem | null;
}

@Component({
  selector: 'app-candidate-filter-bar',
  imports: [
    ReactiveFormsModule,
    HlmInputImports,
    HlmSelectImports,
    HlmComboboxImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSpinnerImports,
  ],
  providers: [provideIcons({ lucideSearch })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-filter-bar.html',
})
export class CandidateFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);
  private readonly _coursesService = inject(CoursesService);
  private readonly _batchesService = inject(BatchesService);

  readonly initial = input<CandidateFilterValue>({
    search: '',
    status: null,
    course: null,
    batch: null,
  });
  readonly filterChanged = output<CandidateFilterValue>();

  protected readonly statuses = CandidateStatus;
  protected readonly statusLabel = (value: StatusFilterValue): string => {
    if (value === 'all') return 'All Status';
    return CANDIDATE_STATUS_LABELS[value];
  };

  protected readonly courseSearch = signal('');
  protected readonly batchSearch = signal('');

  protected readonly courses = this._coursesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.courseSearch(),
      isActive: true,
    })),
  );

  protected readonly batches = this._batchesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.batchSearch(),
    })),
  );

  protected readonly courseItemToString = (c: CourseListItem | null): string => c?.courseName ?? '';
  protected readonly batchItemToString = (b: BatchListItem | null): string =>
    b ? `${b.batchCode} (${b.courseName})` : '';

  readonly form = this._fb.group({
    search: this._fb.nonNullable.control(''),
    status: this._fb.nonNullable.control<StatusFilterValue>('all'),
    course: this._fb.control<CourseListItem | null>(null),
    batch: this._fb.control<BatchListItem | null>(null),
  });

  ngOnInit(): void {
    const v = this.initial();
    this.form.patchValue(
      {
        search: v.search,
        status: v.status ?? 'all',
        course: v.course,
        batch: v.batch,
      },
      { emitEvent: false },
    );

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.status.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.course.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.batch.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    const { search, status, course, batch } = this.form.getRawValue();
    this.filterChanged.emit({
      search: search ?? '',
      status: status === 'all' ? null : (status as CandidateStatus),
      course: course ?? null,
      batch: batch ?? null,
    });
  }
}
