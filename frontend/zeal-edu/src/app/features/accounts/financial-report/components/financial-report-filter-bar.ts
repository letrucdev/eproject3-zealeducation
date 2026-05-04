import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import { lucideSearch } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { FeeType, PaymentMethod } from '@core/models/payment-enums';

export interface FinancialReportFilters {
  search: string;
  feeType: FeeType | null;
  method: PaymentMethod | null;
}

type FeeTypeFilter = FeeType | 'all';
type MethodFilter = PaymentMethod | 'all';

@Component({
  selector: 'app-financial-report-filter-bar',
  imports: [ReactiveFormsModule, HlmIconImports, HlmInputImports, HlmSelectImports],
  providers: [provideIcons({ lucideSearch })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'financial-report-filter-bar.html',
})
export class FinancialReportFilterBar {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly search = input<string>('');
  readonly feeType = input<FeeType | null>(null);
  readonly method = input<PaymentMethod | null>(null);

  readonly filtersChanged = output<FinancialReportFilters>();

  protected readonly feeTypes = FeeType;
  protected readonly paymentMethods = PaymentMethod;

  protected readonly form = this._fb.nonNullable.group({
    search: '',
    feeType: 'all' as FeeTypeFilter,
    method: 'all' as MethodFilter,
  });

  // Tranh re-emit khi parent set lai input
  private readonly _suppressEmit = signal(false);

  protected readonly feeTypeLabel = (value: FeeTypeFilter): string =>
    value === 'all' ? 'All Types' : value;

  protected readonly methodLabel = (value: MethodFilter): string => {
    switch (value) {
      case 'all':
        return 'All Methods';
      case PaymentMethod.BankTransfer:
        return 'Bank Transfer';
      default:
        return value;
    }
  };

  constructor() {
    effect(() => {
      const search = this.search();
      const feeType = this.feeType();
      const method = this.method();

      this._suppressEmit.set(true);
      this.form.patchValue(
        {
          search,
          feeType: feeType ?? 'all',
          method: method ?? 'all',
        },
        { emitEvent: false },
      );
      this._suppressEmit.set(false);
    });

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emitFilters());

    this.form.controls.feeType.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emitFilters());

    this.form.controls.method.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emitFilters());
  }

  private _emitFilters(): void {
    if (this._suppressEmit()) return;
    const value = this.form.getRawValue();
    this.filtersChanged.emit({
      search: value.search,
      feeType: value.feeType === 'all' ? null : value.feeType,
      method: value.method === 'all' ? null : value.method,
    });
  }
}
