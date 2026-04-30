export function calculateLetterGrade(
  score: number | null,
  maxScore: number,
  passScore: number,
): string {
  if (score == null || maxScore <= 0) return '';
  if (score < passScore) return 'F';
  const pct = (score / maxScore) * 100;
  if (pct >= 90) return 'A';
  if (pct >= 80) return 'B';
  if (pct >= 70) return 'C';
  return 'D';
}
