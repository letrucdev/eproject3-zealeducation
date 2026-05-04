import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideDownload } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { toast } from '@spartan-ng/brain/sonner';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
} from '@shared/components/data-table';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import {
  FinancialReportRange,
  FinancialTransactionListItem,
  FinancialTransactionsQuery,
} from '@core/models/financial-report';
import { FeeType, PaymentMethod } from '@core/models/payment-enums';
import { PaymentsService } from '@core/services/payments.service';
import { DateRangeFilterBar, DateRangePreset } from './components/date-range-filter-bar';
import {
  FinancialReportFilterBar,
  FinancialReportFilters,
} from './components/financial-report-filter-bar';
import { FinancialStatsCards } from './components/financial-stats-cards';
import { PaymentStatusDistributionChart } from './components/payment-status-distribution-chart';
import { RevenueByFeeTypeChart } from './components/revenue-by-fee-type-chart';
import { RevenueTrendChart } from './components/revenue-trend-chart';
import { TopCoursesRevenueChart } from './components/top-courses-revenue-chart';

@Directive({
  selector: '[transactionCell]',
  providers: [{ provide: DataTableCellDef, useExisting: TransactionCellDef }],
})
export class TransactionCellDef extends DataTableCellDef<FinancialTransactionListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'transactionCell' });

  static override ngTemplateContextGuard(
    _dir: TransactionCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<FinancialTransactionListItem> {
    return true;
  }
}

function buildRangeFromPreset(preset: Exclude<DateRangePreset, 'custom'>): FinancialReportRange {
  const now = new Date();
  const to = new Date(now);
  to.setHours(23, 59, 59, 999);
  const from = new Date(now);
  from.setDate(from.getDate() - (preset - 1));
  from.setHours(0, 0, 0, 0);
  return { from: from.toISOString(), to: to.toISOString() };
}

class ChartFilterState {
  readonly preset = signal<DateRangePreset>(30);
  readonly range = signal<FinancialReportRange>(buildRangeFromPreset(30));

  readonly onPresetChanged = (value: DateRangePreset): void => {
    this.preset.set(value);
    if (value !== 'custom') {
      this.range.set(buildRangeFromPreset(value));
    }
  };

  readonly onCustomRangeChanged = (range: FinancialReportRange): void => {
    this.preset.set('custom');
    this.range.set(range);
  };
}

@Component({
  selector: 'app-financial-report-page',
  imports: [
    DataTable,
    TransactionCellDef,
    VndPipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmCardImports,
    HlmIconImports,
    HlmSpinnerImports,
    DateRangeFilterBar,
    FinancialReportFilterBar,
    FinancialStatsCards,
    RevenueTrendChart,
    PaymentStatusDistributionChart,
    RevenueByFeeTypeChart,
    TopCoursesRevenueChart,
  ],
  providers: [provideIcons({ lucideDownload })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'financial-report-page.html',
})
export default class FinancialReportPage {
  private readonly _service = inject(PaymentsService);

  protected readonly revenueTrendFilter = new ChartFilterState();
  protected readonly paymentStatusFilter = new ChartFilterState();
  protected readonly revenueByFeeTypeFilter = new ChartFilterState();
  protected readonly topCoursesFilter = new ChartFilterState();

  protected readonly tableDateRange = signal<FinancialReportRange>(buildRangeFromPreset(30));

  protected readonly searchValue = signal<string>('');
  protected readonly feeTypeFilter = signal<FeeType | null>(null);
  protected readonly methodFilter = signal<PaymentMethod | null>(null);

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly exporting = signal(false);

  protected readonly paymentMethods = PaymentMethod;

  protected readonly revenueTrendQuery = this._service.financialReportQuery(
    this.revenueTrendFilter.range,
  );
  protected readonly paymentStatusQuery = this._service.financialReportQuery(
    this.paymentStatusFilter.range,
  );
  protected readonly revenueByFeeTypeQuery = this._service.financialReportQuery(
    this.revenueByFeeTypeFilter.range,
  );
  protected readonly topCoursesQuery = this._service.financialReportQuery(
    this.topCoursesFilter.range,
  );

  private readonly _transactionsParams = computed<FinancialTransactionsQuery>(() => ({
    from: this.tableDateRange().from,
    to: this.tableDateRange().to,
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.searchValue() || undefined,
    feeType: this.feeTypeFilter(),
    method: this.methodFilter(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));
  protected readonly transactionsQuery = this._service.financialTransactionsQuery(
    this._transactionsParams,
  );

  // Card metrics (monthly profit, yearly income, outstanding, transaction count) khong phu thuoc range
  // — tan dung revenueTrendQuery vi no da goi chung endpoint.
  protected readonly summary = computed(() => this.revenueTrendQuery.data() ?? null);

  protected readonly columns: DataTableColumn<FinancialTransactionListItem>[] = [
    { key: 'receiptNumber', header: 'Receipt No.', sortable: true, width: 'w-32' },
    { key: 'paymentDate', header: 'Date', sortable: true, width: 'w-40' },
    { key: 'candidateCode', header: 'Code', width: 'w-28' },
    { key: 'candidateFullName', header: 'Candidate', sortable: true, width: 'w-44' },
    { key: 'courseTitle', header: 'Course', width: 'w-56' },
    { key: 'feeType', header: 'Type', width: 'w-24' },
    { key: 'paymentMethod', header: 'Method', width: 'w-32' },
    { key: 'amount', header: 'Amount', sortable: true, width: 'w-32', align: 'right' },
    { key: 'processedByStaffName', header: 'Processed By', width: 'w-40' },
  ];

  protected readonly trackById = (row: FinancialTransactionListItem): string => row.transactionId;

  onTableCustomRangeChanged(range: FinancialReportRange): void {
    this.tableDateRange.set(range);
    this.page.set(1);
  }

  onTableRangeCleared(): void {
    this.tableDateRange.set(buildRangeFromPreset(30));
    this.page.set(1);
  }

  onFiltersChanged(filters: FinancialReportFilters): void {
    this.searchValue.set(filters.search);
    this.feeTypeFilter.set(filters.feeType);
    this.methodFilter.set(filters.method);
    this.page.set(1);
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

  async onExportExcel(): Promise<void> {
    if (this.exporting()) return;
    this.exporting.set(true);
    try {
      await this._service.downloadFinancialReportExcel({
        revenueTrend: this.revenueTrendFilter.range(),
        paymentStatus: this.paymentStatusFilter.range(),
        revenueByFeeType: this.revenueByFeeTypeFilter.range(),
        topCourses: this.topCoursesFilter.range(),
        transactions: this.tableDateRange(),
        search: this.searchValue() || undefined,
        feeType: this.feeTypeFilter(),
        method: this.methodFilter(),
      });
      toast.success('Financial report exported successfully.');
    } catch {
      toast.error('Failed to export financial report. Please try again.');
    } finally {
      this.exporting.set(false);
    }
  }

  protected formatPaymentMethod(method: PaymentMethod): string {
    return method === PaymentMethod.BankTransfer ? 'Bank Transfer' : method;
  }

  protected formatDate(iso: string): string {
    const date = new Date(iso);
    if (Number.isNaN(date.getTime())) return iso;
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
