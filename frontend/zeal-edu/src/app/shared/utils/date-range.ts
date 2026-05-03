const DATE_FORMAT_OPTIONS: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
};

export function fromIsoDate(value: string | null | undefined): Date | null {
  if (!value) return null;
  const parts = value.split('-');
  if (parts.length !== 3) return null;
  const [y, m, d] = parts.map(Number);
  if (!y || !m || !d) return null;
  return new Date(y, m - 1, d);
}

export function toIsoDate(value: Date): string {
  const yyyy = value.getFullYear();
  const mm = String(value.getMonth() + 1).padStart(2, '0');
  const dd = String(value.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}

export function formatDateRange(dates: [Date | undefined, Date | undefined]): string {
  const [start, end] = dates;
  if (!start && !end) return '';
  const fmt = (d: Date | undefined): string =>
    d ? d.toLocaleDateString('en-US', DATE_FORMAT_OPTIONS) : '';
  return [fmt(start), fmt(end)].filter(Boolean).join(' - ');
}
