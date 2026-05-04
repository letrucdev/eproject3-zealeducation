import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DonutChartComponent, DonutType, LegendPosition } from 'angular-chrts';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { FeeTypeRevenue } from '@core/models/financial-report';
import { FeeType } from '@core/models/payment-enums';

const vndPipe = new VndPipe();

interface FeeTypeRow {
  key: FeeType;
  label: string;
  amount: number;
  percent: number;
  color: string;
}

const FEE_TYPE_DEFS: readonly { key: FeeType; label: string; color: string }[] = [
  { key: FeeType.Tuition, label: 'Tuition', color: 'var(--chart-1)' },
  { key: FeeType.Fine, label: 'Fine', color: 'var(--chart-4)' },
] as const;

@Component({
  selector: 'app-revenue-by-fee-type-chart',
  imports: [DonutChartComponent, HlmCardImports, HlmSkeletonImports, VndPipe],
  host: { class: 'block w-full min-w-0' },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'revenue-by-fee-type-chart.html',
})
export class RevenueByFeeTypeChart {
  readonly data = input<FeeTypeRevenue[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly donutType = DonutType.Full;
  protected readonly legendPosition = LegendPosition.BottomCenter;

  protected readonly total = computed(() =>
    (this.data() ?? []).reduce((sum, b) => sum + b.amount, 0),
  );

  protected readonly hasData = computed(() => this.total() > 0);

  protected readonly rows = computed<FeeTypeRow[]>(() => {
    const buckets = this.data() ?? [];
    const total = this.total();
    if (total === 0) return [];
    return FEE_TYPE_DEFS.map((def) => {
      const bucket = buckets.find((b) => b.feeType === def.key);
      const amount = bucket?.amount ?? 0;
      return {
        key: def.key,
        label: def.label,
        amount,
        percent: (amount / total) * 100,
        color: def.color,
      };
    }).filter((row) => row.amount > 0);
  });

  protected readonly chartValues = computed(() => this.rows().map((r) => r.amount));

  protected readonly categories = computed(() =>
    this.rows().reduce<Record<string, { name: string; color: string }>>((acc, row) => {
      acc[`fee-${row.key}`] = { name: row.label, color: row.color };
      return acc;
    }, {}),
  );

  protected readonly tooltipTitleFormatter = (
    data: { label?: string; [key: string]: unknown } | undefined,
  ): string => {
    if (!data?.label) return '';
    const amount = Number(data[data.label]);
    if (!Number.isFinite(amount)) return data.label;
    const formatted = vndPipe.transform(amount) ?? `${amount}`;
    const total = this.total();
    if (total === 0) return `${data.label}: ${formatted}`;
    const pct = ((amount / total) * 100).toFixed(1);
    return `${data.label}: ${formatted} (${pct}%)`;
  };
}
