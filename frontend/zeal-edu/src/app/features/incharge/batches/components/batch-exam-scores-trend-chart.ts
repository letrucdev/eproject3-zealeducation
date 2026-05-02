import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { AreaChartComponent } from 'angular-chrts';
import { HlmButtonGroupImports } from '@spartan-ng/helm/button-group';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { HlmToggleImports } from '@spartan-ng/helm/toggle';
import {
  BatchExamScoresTrendPoint,
  BatchExamScoresTrendRange,
} from '../models/batch-exam-scores-trend';

interface RangeOption {
  value: BatchExamScoresTrendRange;
  label: string;
}

const RANGE_OPTIONS: readonly RangeOption[] = [
  { value: 90, label: 'Last 3 months' },
  { value: 30, label: 'Last 30 days' },
  { value: 7, label: 'Last 7 days' },
] as const;

const SUBTITLES: Record<BatchExamScoresTrendRange, string> = {
  90: 'Exam scores over the last 3 months',
  30: 'Exam scores over the last 30 days',
  7: 'Exam scores over the last 7 days',
};

@Component({
  selector: 'app-batch-exam-scores-trend-chart',
  imports: [
    AreaChartComponent,
    HlmButtonGroupImports,
    HlmCardImports,
    HlmSkeletonImports,
    HlmToggleImports,
  ],
  host: { class: 'block w-full min-w-0' },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-exam-scores-trend-chart.html',
})
export class BatchExamScoresTrendChart {
  readonly data = input<BatchExamScoresTrendPoint[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly range = input.required<BatchExamScoresTrendRange>();
  readonly rangeChanged = output<BatchExamScoresTrendRange>();

  protected readonly rangeOptions = RANGE_OPTIONS;

  protected readonly subtitle = computed(() => SUBTITLES[this.range()]);

  protected readonly chartData = computed(() => this.data() ?? []);

  protected readonly hasData = computed(() => this.chartData().length > 0);

  protected readonly categories = computed(() => ({
    averageScore: { name: 'Average', color: 'var(--chart-1)' },
    highestScore: { name: 'Highest', color: 'var(--chart-3)' },
    lowestScore: { name: 'Lowest', color: 'var(--chart-4)' },
  }));

  protected readonly xFormatter = (value: number | Date): string => {
    const points = this.chartData();
    const idx = typeof value === 'number' ? Math.round(value) : 0;
    const point = points[idx];
    if (!point) return '';
    return this._formatDate(point.date);
  };

  protected readonly tooltipTitleFormatter = (point: BatchExamScoresTrendPoint): string =>
    this._formatDate(point.date);

  protected onToggleState(value: BatchExamScoresTrendRange, state: 'on' | 'off'): void {
    if (state === 'on' && value !== this.range()) {
      this.rangeChanged.emit(value);
    }
  }

  protected toggleState(value: BatchExamScoresTrendRange): 'on' | 'off' {
    return this.range() === value ? 'on' : 'off';
  }

  private _formatDate(iso: string): string {
    const date = new Date(`${iso}T00:00:00`);
    if (Number.isNaN(date.getTime())) return iso;
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
