import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DonutChartComponent, DonutType, LegendPosition } from 'angular-chrts';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { PaymentStatusBucket } from '@core/models/financial-report';
import { PaymentStatus } from '@core/models/payment-enums';

interface StatusRow {
  key: PaymentStatus;
  label: string;
  count: number;
  percent: number;
  color: string;
}

const STATUS_DEFS: readonly { key: PaymentStatus; label: string; color: string }[] = [
  { key: PaymentStatus.Paid, label: 'Paid', color: 'var(--chart-3)' },
  { key: PaymentStatus.Partial, label: 'Partial', color: 'var(--chart-2)' },
  { key: PaymentStatus.Unpaid, label: 'Unpaid', color: 'var(--chart-1)' },
  { key: PaymentStatus.Overdue, label: 'Overdue', color: 'var(--chart-4)' },
] as const;

@Component({
  selector: 'app-payment-status-distribution-chart',
  imports: [DecimalPipe, DonutChartComponent, HlmCardImports, HlmSkeletonImports],
  host: { class: 'block w-full min-w-0' },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'payment-status-distribution-chart.html',
})
export class PaymentStatusDistributionChart {
  readonly data = input<PaymentStatusBucket[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly donutType = DonutType.Full;
  protected readonly legendPosition = LegendPosition.BottomCenter;

  protected readonly total = computed(() =>
    (this.data() ?? []).reduce((sum, b) => sum + b.count, 0),
  );

  protected readonly hasData = computed(() => this.total() > 0);

  protected readonly rows = computed<StatusRow[]>(() => {
    const buckets = this.data() ?? [];
    const total = this.total();
    if (total === 0) return [];
    return STATUS_DEFS.map((def) => {
      const bucket = buckets.find((b) => b.status === def.key);
      const count = bucket?.count ?? 0;
      return {
        key: def.key,
        label: def.label,
        count,
        percent: (count / total) * 100,
        color: def.color,
      };
    }).filter((row) => row.count > 0);
  });

  protected readonly chartValues = computed(() => this.rows().map((r) => r.count));

  protected readonly categories = computed(() =>
    this.rows().reduce<Record<string, { name: string; color: string }>>((acc, row) => {
      acc[row.key] = { name: row.label, color: row.color };
      return acc;
    }, {}),
  );

  protected readonly tooltipTitleFormatter = (
    data: { label?: string; [key: string]: unknown } | undefined,
  ): string => {
    if (!data?.label) return '';
    const count = Number(data[data.label]);
    if (!Number.isFinite(count)) return '';
    const total = this.total();
    if (total === 0) return `${data.label}: ${count}`;
    const pct = ((count / total) * 100).toFixed(1);
    return `${data.label}: ${count} (${pct}%)`;
  };
}
