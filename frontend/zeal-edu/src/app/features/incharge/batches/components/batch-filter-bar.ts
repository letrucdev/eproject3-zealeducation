import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import { lucidePlus, lucideSearch } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { BatchStatus } from '@core/models/batch-status';
import { BATCH_STATUS_LABELS } from './batch-labels';

type StatusFilterValue = BatchStatus | 'all';

export interface BatchFilterValue {
  search: string;
  status: BatchStatus | null;
}

interface BatchFilterForm {
  search: string;
  status: StatusFilterValue;
}

@Component({
  selector: 'app-batch-filter-bar',
  imports: [
    ReactiveFormsModule,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideSearch, lucidePlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-filter-bar.html',
})
export class BatchFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input<BatchFilterValue>({ search: '', status: null });
  readonly filterChanged = output<BatchFilterValue>();
  readonly createClicked = output<void>();

  protected readonly statuses = BatchStatus;

  protected readonly statusLabel = (value: StatusFilterValue): string => {
    if (value === 'all') return 'All Status';
    return BATCH_STATUS_LABELS[value];
  };

  readonly form = this._fb.nonNullable.group<BatchFilterForm>({
    search: '',
    status: 'all',
  });

  ngOnInit(): void {
    const v = this.initial();
    this.form.patchValue(
      {
        search: v.search,
        status: v.status ?? 'all',
      },
      { emitEvent: false },
    );

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.status.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    const { search, status } = this.form.getRawValue();
    this.filterChanged.emit({
      search,
      status: status === 'all' ? null : status,
    });
  }
}
