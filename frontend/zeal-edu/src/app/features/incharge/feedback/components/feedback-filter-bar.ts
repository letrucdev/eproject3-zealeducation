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
import { BatchListItem } from '@core/models/batch-list-item';
import { BatchesService } from '@features/incharge/batches/batches.service';
import { FeedbackType } from '../models/feedback-list-item';

type TypeFilterValue = FeedbackType | 'all';
type RatingFilterValue = number | 'all';
export type ProcessedView = 'unprocessed' | 'processed' | 'all';

export interface FeedbackFilterValue {
  search: string;
  type: FeedbackType | null;
  batch: BatchListItem | null;
  rating: number | null;
  processedView: ProcessedView;
}

@Component({
  selector: 'app-feedback-filter-bar',
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
  templateUrl: 'feedback-filter-bar.html',
})
export class FeedbackFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);
  private readonly _batchesService = inject(BatchesService);

  readonly initial = input<FeedbackFilterValue>({
    search: '',
    type: null,
    batch: null,
    rating: null,
    processedView: 'unprocessed',
  });
  readonly filterChanged = output<FeedbackFilterValue>();

  protected readonly typeLabel = (value: TypeFilterValue): string =>
    value === 'all' ? 'All Types' : value;

  protected readonly ratingLabel = (value: RatingFilterValue): string =>
    value === 'all' ? 'All Ratings' : `${value} ★`;

  protected readonly processedLabel = (value: ProcessedView): string => {
    switch (value) {
      case 'unprocessed':
        return 'Unprocessed only';
      case 'processed':
        return 'Processed only';
      case 'all':
        return 'All';
    }
  };

  protected readonly batchSearch = signal('');

  protected readonly batches = this._batchesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.batchSearch(),
    })),
  );

  protected readonly batchItemToString = (b: BatchListItem | null): string =>
    b ? `${b.batchCode} (${b.courseName})` : '';

  readonly form = this._fb.group({
    search: this._fb.nonNullable.control(''),
    type: this._fb.nonNullable.control<TypeFilterValue>('all'),
    batch: this._fb.control<BatchListItem | null>(null),
    rating: this._fb.nonNullable.control<RatingFilterValue>('all'),
    processedView: this._fb.nonNullable.control<ProcessedView>('unprocessed'),
  });

  ngOnInit(): void {
    const v = this.initial();
    this.form.patchValue(
      {
        search: v.search,
        type: v.type ?? 'all',
        batch: v.batch,
        rating: v.rating ?? 'all',
        processedView: v.processedView,
      },
      { emitEvent: false },
    );

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.type.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.batch.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.rating.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.processedView.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    const { search, type, batch, rating, processedView } = this.form.getRawValue();
    this.filterChanged.emit({
      search: search ?? '',
      type: type === 'all' ? null : (type as FeedbackType),
      batch: batch ?? null,
      rating: rating === 'all' ? null : (rating as number),
      processedView,
    });
  }
}
