export function parseFileNameFromContentDisposition(header: string | null): string | null {
  if (!header) return null;
  const utf8Match = /filename\*=UTF-8''([^;]+)/i.exec(header);
  if (utf8Match?.[1]) {
    try {
      return decodeURIComponent(utf8Match[1]);
    } catch {
      // ignore decode errors and fall through
    }
  }
  const asciiMatch = /filename="?([^";]+)"?/i.exec(header);
  return asciiMatch?.[1] ?? null;
}
