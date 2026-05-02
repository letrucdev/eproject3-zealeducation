import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DonutChartComponent, DonutType, LegendPosition } from 'angular-chrts';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { BatchGradeDistribution } from '../models/batch-grade-distribution';

interface GradeRow {
  key: keyof Omit<BatchGradeDistribution, 'total'>;
  label: string;
  count: number;
  percent: number;
  color: string;
}

const GRADE_DEFS: readonly {
  key: keyof Omit<BatchGradeDistribution, 'total'>;
  label: string;
  color: string;
}[] = [
  { key: 'a', label: 'A', color: 'var(--chart-3)' },
  { key: 'b', label: 'B', color: 'var(--chart-2)' },
  { key: 'c', label: 'C', color: 'var(--chart-1)' },
  { key: 'd', label: 'D', color: 'var(--chart-4)' },
  { key: 'f', label: 'F', color: 'var(--chart-5)' },
  { key: 'ungraded', label: 'Ungraded', color: 'var(--muted-foreground)' },
] as const;

@Component({
  selector: 'app-batch-grade-distribution-chart',
  imports: [DecimalPipe, DonutChartComponent, HlmCardImports, HlmSkeletonImports],
  host: { class: 'block w-full min-w-0' },
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-grade-distribution-chart.html',
})
export class BatchGradeDistributionChart {
  readonly data = input<BatchGradeDistribution | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly donutType = DonutType.Full;
  protected readonly legendPosition = LegendPosition.BottomCenter;

  protected readonly total = computed(() => this.data()?.total ?? 0);

  protected readonly hasData = computed(() => this.total() > 0);

  protected readonly rows = computed<GradeRow[]>(() => {
    const dist = this.data();
    const total = this.total();
    if (!dist || total === 0) return [];
    return GRADE_DEFS.map((def) => {
      const count = dist[def.key];
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
      acc[row.key] = { name: `Grade ${row.label}`, color: row.color };
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
    return `${data.label}: ${count} of ${total} (${pct}%)`;
  };
}
