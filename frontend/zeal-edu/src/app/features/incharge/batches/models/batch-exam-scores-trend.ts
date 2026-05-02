export type BatchExamScoresTrendRange = 7 | 30 | 90;

export interface BatchExamScoresTrendPoint {
  date: string;
  averageScore: number;
  highestScore: number;
  lowestScore: number;
  count: number;
}
