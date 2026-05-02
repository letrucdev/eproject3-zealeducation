import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { AreaChartComponent, CurveType } from 'angular-chrts';
import { HlmButtonGroupImports } from '@spartan-ng/helm/button-group';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { HlmToggleImports } from '@spartan-ng/helm/toggle';
import { BatchCreationTrendPoint, BatchCreationTrendRange } from '../models/batch-creation-trend';

interface RangeOption {
  value: BatchCreationTrendRange;
  label: string;
}

const RANGE_OPTIONS: readonly RangeOption[] = [
  { value: 90, label: 'Last 3 months' },
  { value: 30, label: 'Last 30 days' },
  { value: 7, label: 'Last 7 days' },
] as const;

const SUBTITLES: Record<BatchCreationTrendRange, string> = {
  90: 'Total for the last 3 months',
  30: 'Total for the last 30 days',
  7: 'Total for the last 7 days',
};

@Component({
  selector: 'app-batch-creation-trend-chart',
  imports: [
    AreaChartComponent,
    HlmButtonGroupImports,
    HlmCardImports,
    HlmSkeletonImports,
    HlmToggleImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-creation-trend-chart.html',
})
export class BatchCreationTrendChart {
  readonly data = input<BatchCreationTrendPoint[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly range = input.required<BatchCreationTrendRange>();
  readonly rangeChanged = output<BatchCreationTrendRange>();

  protected readonly curveType = CurveType;
  protected readonly rangeOptions = RANGE_OPTIONS;

  protected readonly subtitle = computed(() => SUBTITLES[this.range()]);

  protected readonly chartData = computed(() => this.data() ?? []);

  protected readonly hasData = computed(() => this.chartData().some((p) => p.total > 0));

  protected readonly categories = computed(() => ({
    total: { name: 'Total', color: 'var(--chart-1)' },
    active: { name: 'Active', color: 'var(--chart-2)' },
    completed: { name: 'Completed', color: 'var(--chart-3)' },
    cancelled: { name: 'Cancelled', color: 'var(--chart-4)' },
  }));

  protected readonly xFormatter = (value: number | Date): string => {
    const points = this.chartData();
    const idx = typeof value === 'number' ? Math.round(value) : 0;
    const point = points[idx];
    if (!point) return '';
    return this._formatDate(point.date);
  };

  protected readonly tooltipTitleFormatter = (point: BatchCreationTrendPoint): string =>
    this._formatDate(point.date);

  protected onToggleState(value: BatchCreationTrendRange, state: 'on' | 'off'): void {
    if (state === 'on' && value !== this.range()) {
      this.rangeChanged.emit(value);
    }
  }

  protected toggleState(value: BatchCreationTrendRange): 'on' | 'off' {
    return this.range() === value ? 'on' : 'off';
  }

  private _formatDate(iso: string): string {
    const date = new Date(`${iso}T00:00:00`);
    if (Number.isNaN(date.getTime())) return iso;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
