export type BatchCreationTrendRange = 7 | 30 | 90;

export interface BatchCreationTrendPoint {
  date: string;
  total: number;
  active: number;
  completed: number;
  cancelled: number;
}
