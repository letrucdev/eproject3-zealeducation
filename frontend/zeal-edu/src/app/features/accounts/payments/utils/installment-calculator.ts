import { InstallmentFrequency } from '@core/models/payment-enums';

export const INSTALLMENT_THRESHOLD_WEEKS: Record<InstallmentFrequency, number> = {
  [InstallmentFrequency.Monthly]: 8,
  [InstallmentFrequency.Quarterly]: 24,
  [InstallmentFrequency.BiYearly]: 48,
};

export interface PlannedInstallment {
  installmentNo: number;
  amountDue: number;
  dueDate: string;
}

export function isFrequencyAllowed(frequency: InstallmentFrequency, durationWeeks: number): boolean {
  return durationWeeks >= INSTALLMENT_THRESHOLD_WEEKS[frequency];
}

export function availableFrequencies(durationWeeks: number): InstallmentFrequency[] {
  return [
    InstallmentFrequency.Monthly,
    InstallmentFrequency.Quarterly,
    InstallmentFrequency.BiYearly,
  ].filter((f) => isFrequencyAllowed(f, durationWeeks));
}

function intervalMonths(frequency: InstallmentFrequency): number {
  switch (frequency) {
    case InstallmentFrequency.Monthly:
      return 1;
    case InstallmentFrequency.Quarterly:
      return 3;
    case InstallmentFrequency.BiYearly:
      return 6;
  }
}

function addMonths(baseIso: string, monthsToAdd: number): string {
  const [y, m, d] = baseIso.split('-').map((s) => parseInt(s, 10));
  const date = new Date(Date.UTC(y, (m - 1) + monthsToAdd, d));
  return date.toISOString().substring(0, 10);
}

function round2(n: number): number {
  return Math.round(n * 100) / 100;
}

export function calculateInstallmentPreview(
  totalFee: number,
  durationWeeks: number,
  enrollmentDate: string,
  frequency: InstallmentFrequency,
): PlannedInstallment[] {
  if (!isFrequencyAllowed(frequency, durationWeeks)) return [];

  const months = Math.max(1, Math.floor(durationWeeks / 4));
  const interval = intervalMonths(frequency);
  const count = Math.max(1, Math.ceil(months / interval));

  const amountPer = round2(totalFee / count);
  const distributed = round2(amountPer * (count - 1));
  const lastAmount = round2(totalFee - distributed);

  const result: PlannedInstallment[] = [];
  for (let i = 0; i < count; i++) {
    result.push({
      installmentNo: i + 1,
      amountDue: i === count - 1 ? lastAmount : amountPer,
      dueDate: addMonths(enrollmentDate, interval * (i + 1)),
    });
  }
  return result;
}

export function frequencyLabel(frequency: InstallmentFrequency): string {
  switch (frequency) {
    case InstallmentFrequency.Monthly:
      return 'Monthly (every 1 month)';
    case InstallmentFrequency.Quarterly:
      return 'Quarterly (every 3 months)';
    case InstallmentFrequency.BiYearly:
      return 'Bi-yearly (every 6 months)';
  }
}
