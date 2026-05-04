import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { HlmButtonGroupImports } from '@spartan-ng/helm/button-group';
import { HlmDatePickerImports } from '@spartan-ng/helm/date-picker';
import { HlmToggleImports } from '@spartan-ng/helm/toggle';
import { FinancialReportRange } from '@core/models/financial-report';

export type DateRangePreset = 7 | 30 | 90 | 'custom';

interface PresetOption {
  value: DateRangePreset;
  label: string;
}

const PRESETS: readonly PresetOption[] = [
  { value: 7, label: 'Last 7 days' },
  { value: 30, label: 'Last 30 days' },
  { value: 90, label: 'Last 90 days' },
  { value: 'custom', label: 'Custom' },
] as const;

const DATE_FORMAT_OPTIONS: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
};

@Component({
  selector: 'app-date-range-filter-bar',
  imports: [HlmButtonGroupImports, HlmDatePickerImports, HlmToggleImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'date-range-filter-bar.html',
})
export class DateRangeFilterBar {
  readonly preset = input<DateRangePreset>('custom');
  readonly dateRange = input.required<FinancialReportRange>();
  readonly ariaLabel = input<string>('Select date range preset');
  readonly showPresets = input<boolean>(true);

  readonly presetChanged = output<DateRangePreset>();
  readonly customRangeChanged = output<FinancialReportRange>();
  readonly cleared = output<void>();

  protected readonly presetOptions = PRESETS;

  protected readonly showPicker = computed(
    () => !this.showPresets() || this.preset() === 'custom',
  );

  protected readonly pickerValue = computed<[Date, Date]>(() => {
    const range = this.dateRange();
    return [new Date(range.from), new Date(range.to)];
  });

  protected readonly formatDates = (dates: [Date | undefined, Date | undefined]): string => {
    const [start, end] = dates;
    if (!start && !end) return '';
    const fmt = (d: Date | undefined) => (d ? d.toLocaleDateString('en-US', DATE_FORMAT_OPTIONS) : '');
    return [fmt(start), fmt(end)].filter(Boolean).join(' - ');
  };

  protected onPresetState(value: DateRangePreset, state: 'on' | 'off'): void {
    if (state === 'on' && value !== this.preset()) {
      this.presetChanged.emit(value);
    }
  }

  protected presetState(value: DateRangePreset): 'on' | 'off' {
    return this.preset() === value ? 'on' : 'off';
  }

  protected onDateChange(value: [Date, Date] | null): void {
    if (!value) {
      this.cleared.emit();
      return;
    }
    const [start, end] = value;
    const from = new Date(start);
    from.setHours(0, 0, 0, 0);
    const to = new Date(end);
    to.setHours(23, 59, 59, 999);
    this.customRangeChanged.emit({
      from: from.toISOString(),
      to: to.toISOString(),
    });
  }
}
