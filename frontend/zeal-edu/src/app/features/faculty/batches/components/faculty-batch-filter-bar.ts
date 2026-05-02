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
import { lucideSearch } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { BatchStatus } from '@core/models/batch-status';
import { BATCH_STATUS_LABELS } from '@core/models/batch-labels';

type StatusFilterValue = BatchStatus | 'all';

export interface FacultyBatchFilterValue {
  search: string;
  status: BatchStatus | null;
}

interface FacultyBatchFilterForm {
  search: string;
  status: StatusFilterValue;
}

@Component({
  selector: 'app-faculty-batch-filter-bar',
  imports: [ReactiveFormsModule, HlmInputImports, HlmSelectImports, HlmIconImports],
  providers: [provideIcons({ lucideSearch })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form
      [formGroup]="form"
      class="flex flex-col gap-3 md:flex-row md:flex-wrap md:items-center"
    >
      <div class="relative w-full md:w-96">
        <ng-icon
          hlm
          name="lucideSearch"
          size="sm"
          class="text-muted-foreground pointer-events-none absolute inset-y-0 inset-s-3 my-auto"
        />
        <input
          hlmInput
          type="search"
          class="w-full ps-9"
          placeholder="Search by batch code or location..."
          formControlName="search"
          aria-label="Search batches"
        />
      </div>

      <hlm-select formControlName="status" [itemToString]="statusLabel" class="w-full md:w-52">
        <hlm-select-trigger>
          <hlm-select-value placeholder="Status: All" />
        </hlm-select-trigger>
        <hlm-select-content *hlmSelectPortal>
          <hlm-select-item value="all">All Status</hlm-select-item>
          <hlm-select-item [value]="statuses.NeedsInstructor">Needs Instructor</hlm-select-item>
          <hlm-select-item [value]="statuses.Active">Active</hlm-select-item>
          <hlm-select-item [value]="statuses.Completed">Completed</hlm-select-item>
          <hlm-select-item [value]="statuses.Cancelled">Cancelled</hlm-select-item>
        </hlm-select-content>
      </hlm-select>
    </form>
  `,
})
export class FacultyBatchFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input<FacultyBatchFilterValue>({ search: '', status: null });
  readonly filterChanged = output<FacultyBatchFilterValue>();

  protected readonly statuses = BatchStatus;

  protected readonly statusLabel = (value: StatusFilterValue): string => {
    if (value === 'all') return 'All Status';
    return BATCH_STATUS_LABELS[value];
  };

  readonly form = this._fb.nonNullable.group<FacultyBatchFilterForm>({
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
