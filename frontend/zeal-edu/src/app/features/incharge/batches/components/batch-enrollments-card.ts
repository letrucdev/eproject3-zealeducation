import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Directive,
  OnInit,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucideEye, lucideSearch, lucideUserPlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { BatchEnrollmentItem } from '@core/models/batch-enrollment';
import { EnrollmentStatus } from '@features/incharge/candidates/models/candidate-detail';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';

@Directive({
  selector: '[enrollmentCell]',
  providers: [{ provide: DataTableCellDef, useExisting: EnrollmentCellDef }],
})
export class EnrollmentCellDef extends DataTableCellDef<BatchEnrollmentItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'enrollmentCell' });

  static override ngTemplateContextGuard(
    _dir: EnrollmentCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<BatchEnrollmentItem> {
    return true;
  }
}

@Component({
  selector: 'app-batch-enrollments-card',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    DataTable,
    EnrollmentCellDef,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmInputImports,
  ],
  providers: [provideIcons({ lucideEye, lucideSearch, lucideUserPlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-enrollments-card.html',
})
export class BatchEnrollmentsCard implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly page = input<PaginatedList<BatchEnrollmentItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly canAddCandidate = input<boolean>(true);
  readonly pageSize = input<number>(10);
  readonly search = input<string>('');
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly addClicked = output<void>();
  readonly viewClicked = output<BatchEnrollmentItem>();
  readonly searchChanged = output<string>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly enrollmentStatuses = EnrollmentStatus;

  protected readonly searchControl = this._fb.nonNullable.control('');

  protected readonly columns: DataTableColumn<BatchEnrollmentItem>[] = [
    { key: 'candidateCode', header: 'Code', sortable: true, width: 'w-32' },
    { key: 'fullName', header: 'Full Name', sortable: true, width: 'w-56' },
    { key: 'contact', header: 'Contact', width: 'w-64' },
    { key: 'enrollmentDate', header: 'Enrolled On', sortable: true, width: 'w-40' },
    { key: 'status', header: 'Status', sortable: true, align: 'center', width: 'w-32' },
    { key: 'actions', header: 'Actions', align: 'right', width: 'w-24' },
  ];

  protected readonly trackById = (row: BatchEnrollmentItem): string => row.enrollmentId;

  constructor() {
    effect(() => {
      const next = this.search();
      if (this.searchControl.value !== next) {
        this.searchControl.setValue(next, { emitEvent: false });
      }
    });
  }

  ngOnInit(): void {
    this.searchControl.setValue(this.search(), { emitEvent: false });
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => this.searchChanged.emit(value ?? ''));
  }
}
