import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { AreaChartComponent } from 'angular-chrts';
import { HlmButtonGroupImports } from '@spartan-ng/helm/button-group';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { HlmToggleImports } from '@spartan-ng/helm/toggle';
import {
  CandidateRegistrationTrendPoint,
  RegistrationsTrendRange,
} from '../models/candidate-registration-trend';

interface RangeOption {
  value: RegistrationsTrendRange;
  label: string;
}

const RANGE_OPTIONS: readonly RangeOption[] = [
  { value: 90, label: 'Last 3 months' },
  { value: 30, label: 'Last 30 days' },
  { value: 7, label: 'Last 7 days' },
] as const;

const SUBTITLES: Record<RegistrationsTrendRange, string> = {
  90: 'Total for the last 3 months',
  30: 'Total for the last 30 days',
  7: 'Total for the last 7 days',
};

@Component({
  selector: 'app-registrations-trend-chart',
  imports: [
    AreaChartComponent,
    HlmButtonGroupImports,
    HlmCardImports,
    HlmSkeletonImports,
    HlmToggleImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'registrations-trend-chart.html',
})
export class RegistrationsTrendChart {
  readonly data = input<CandidateRegistrationTrendPoint[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly range = input.required<RegistrationsTrendRange>();
  readonly rangeChanged = output<RegistrationsTrendRange>();

  protected readonly rangeOptions = RANGE_OPTIONS;

  protected readonly subtitle = computed(() => SUBTITLES[this.range()]);

  protected readonly chartData = computed(() => this.data() ?? []);

  protected readonly hasData = computed(() =>
    this.chartData().some((p) => p.count > 0 || p.graduated > 0 || p.dropped > 0),
  );

  protected readonly categories = computed(() => ({
    count: { name: 'New Registrations', color: 'var(--chart-2)' },
    graduated: { name: 'Graduated', color: 'var(--chart-1)' },
    dropped: { name: 'Dropped', color: 'var(--destructive)' },
  }));

  protected readonly xFormatter = (value: number | Date): string => {
    const points = this.chartData();
    const idx = typeof value === 'number' ? Math.round(value) : 0;
    const point = points[idx];
    if (!point) return '';
    return this._formatDate(point.date);
  };

  protected readonly tooltipTitleFormatter = (point: CandidateRegistrationTrendPoint): string =>
    this._formatDate(point.date);

  protected onToggleState(value: RegistrationsTrendRange, state: 'on' | 'off'): void {
    if (state === 'on' && value !== this.range()) {
      this.rangeChanged.emit(value);
    }
  }

  protected toggleState(value: RegistrationsTrendRange): 'on' | 'off' {
    return this.range() === value ? 'on' : 'off';
  }

  private _formatDate(iso: string): string {
    const date = new Date(`${iso}T00:00:00`);
    if (Number.isNaN(date.getTime())) return iso;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
