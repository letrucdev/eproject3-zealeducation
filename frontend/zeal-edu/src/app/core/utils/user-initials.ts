export function getUserInitials(fullName: string | null | undefined): string {
  const name = fullName?.trim() ?? '';
  if (!name) return '?';
  const parts = name.split(/\s+/);
  const first = parts[0]?.[0] ?? '';
  const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
  return (first + last).toUpperCase() || '?';
}
