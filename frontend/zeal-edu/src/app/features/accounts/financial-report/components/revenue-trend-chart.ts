import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { AreaChartComponent, CurveType } from 'angular-chrts';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { RevenueTrendPoint } from '@core/models/financial-report';

@Component({
  selector: 'app-revenue-trend-chart',
  imports: [AreaChartComponent, HlmCardImports, HlmSkeletonImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'revenue-trend-chart.html',
})
export class RevenueTrendChart {
  readonly data = input<RevenueTrendPoint[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly curveType = CurveType;

  protected readonly chartData = computed(() => this.data() ?? []);

  protected readonly hasData = computed(() => this.chartData().some((p) => p.amount > 0));

  protected readonly categories = computed(() => ({
    amount: { name: 'Revenue', color: 'var(--chart-1)' },
  }));

  protected readonly xFormatter = (value: number | Date): string => {
    const points = this.chartData();
    const idx = typeof value === 'number' ? Math.round(value) : 0;
    const point = points[idx];
    if (!point) return '';
    return this._formatDate(point.date);
  };

  protected readonly yFormatter = (value: number | Date): string =>
    this._formatAmount(typeof value === 'number' ? value : 0);

  protected readonly tooltipTitleFormatter = (point: RevenueTrendPoint): string =>
    this._formatDate(point.date);

  private _formatDate(iso: string): string {
    const date = new Date(`${iso}T00:00:00`);
    if (Number.isNaN(date.getTime())) return iso;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  private _formatAmount(value: number): string {
    if (value >= 1_000_000_000) return `${(value / 1_000_000_000).toFixed(1)}B`;
    if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `${(value / 1_000).toFixed(0)}K`;
    return value.toString();
  }
}
