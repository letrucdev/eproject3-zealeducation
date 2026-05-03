import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DonutChartComponent, DonutType, LegendPosition } from 'angular-chrts';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { CourseRevenue } from '@core/models/financial-report';

interface CourseRow {
  courseId: string;
  label: string;
  amount: number;
  percent: number;
  color: string;
}

const PALETTE = [
  'var(--chart-1)',
  'var(--chart-2)',
  'var(--chart-3)',
  'var(--chart-4)',
  'var(--chart-5)',
];

@Component({
  selector: 'app-top-courses-revenue-chart',
  imports: [DonutChartComponent, HlmCardImports, HlmSkeletonImports, VndPipe],
  host: { class: 'block w-full min-w-0' },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'top-courses-revenue-chart.html',
})
export class TopCoursesRevenueChart {
  readonly data = input<CourseRevenue[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly donutType = DonutType.Full;
  protected readonly legendPosition = LegendPosition.BottomCenter;

  protected readonly total = computed(() =>
    (this.data() ?? []).reduce((sum, c) => sum + c.amount, 0),
  );

  protected readonly hasData = computed(() => (this.data() ?? []).length > 0 && this.total() > 0);

  protected readonly rows = computed<CourseRow[]>(() => {
    const items = this.data() ?? [];
    const total = this.total();
    if (total === 0) return [];
    return items.map((c, i) => ({
      courseId: c.courseId,
      label: c.courseTitle,
      amount: c.amount,
      percent: (c.amount / total) * 100,
      color: PALETTE[i % PALETTE.length],
    }));
  });

  protected readonly chartValues = computed(() => this.rows().map((r) => r.amount));

  protected readonly categories = computed(() =>
    this.rows().reduce<Record<string, { name: string; color: string }>>((acc, row) => {
      acc[row.courseId] = { name: row.label, color: row.color };
      return acc;
    }, {}),
  );

  protected readonly tooltipTitleFormatter = (
    data: { label?: string; [key: string]: unknown } | undefined,
  ): string => {
    if (!data?.label) return '';
    const row = this.rows().find((r) => r.label === data.label);
    if (!row) return '';
    return `${row.label}: ${row.percent.toFixed(1)}%`;
  };
}
