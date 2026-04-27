import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Directive,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucideEye, lucideSearch } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
} from '@shared/components/data-table';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { FeeStructureListItem } from './models/fee-structure';
import { FeeType, PaymentStatus, PaymentType } from './models/payment-enums';
import { FeeStructureListQuery } from './models/payment-payload';
import { PaymentsService } from './payments.service';

type StatusFilter = PaymentStatus | 'all';
type TypeFilter = FeeType | 'all';

@Directive({
  selector: '[paymentCell]',
  providers: [{ provide: DataTableCellDef, useExisting: PaymentCellDef }],
})
export class PaymentCellDef extends DataTableCellDef<FeeStructureListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'paymentCell' });

  static override ngTemplateContextGuard(
    _dir: PaymentCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<FeeStructureListItem> {
    return true;
  }
}

@Component({
  selector: 'app-payments-list-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DataTable,
    PaymentCellDef,
    VndPipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmInputImports,
    HlmSelectImports,
  ],
  providers: [provideIcons({ lucideSearch, lucideEye })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'payments-list-page.html',
})
export default class PaymentsListPage {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);
  private readonly _service = inject(PaymentsService);

  protected readonly paymentStatuses = PaymentStatus;
  protected readonly paymentTypes = PaymentType;
  protected readonly feeTypes = FeeType;

  protected readonly statusLabel = (value: StatusFilter): string => {
    if (value === 'all') return 'All Status';
    return value;
  };

  protected readonly typeLabel = (value: TypeFilter): string => {
    if (value === 'all') return 'All Types';
    return value;
  };

  protected readonly form = this._fb.nonNullable.group({
    search: '',
    status: 'all' as StatusFilter,
    type: 'all' as TypeFilter,
  });

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal<string>('');
  protected readonly statusFilter = signal<PaymentStatus | null>(null);
  protected readonly typeFilter = signal<FeeType | null>(null);
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  private readonly _params = computed<FeeStructureListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    status: this.statusFilter(),
    type: this.typeFilter(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.listQuery(this._params);

  protected readonly columns: DataTableColumn<FeeStructureListItem>[] = [
    { key: 'candidateCode', header: 'Code', width: 'w-32' },
    { key: 'candidateFullName', header: 'Candidate', width: 'w-32' },
    { key: 'courseTitle', header: 'Course', width: 'w-56' },
    { key: 'feeType', header: 'Type', width: 'w-28' },
    { key: 'totalFee', header: 'Total', sortable: true, width: 'w-32', align: 'right' },
    { key: 'amountPaid', header: 'Paid', sortable: true, width: 'w-32', align: 'right' },
    { key: 'outstandingBalance', header: 'Outstanding', sortable: true, width: 'w-32', align: 'right' },
    { key: 'paymentStatus', header: 'Status', sortable: true, width: 'w-32', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-20', align: 'right' },
  ];

  protected readonly trackById = (row: FeeStructureListItem): string => row.feeId;

  constructor() {
    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        this.search.set(value);
        this.page.set(1);
      });

    this.form.controls.status.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        this.statusFilter.set(value === 'all' ? null : value);
        this.page.set(1);
      });

    this.form.controls.type.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        this.typeFilter.set(value === 'all' ? null : value);
        this.page.set(1);
      });
  }

  onPageChanged(page: number): void {
    this.page.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }
}
