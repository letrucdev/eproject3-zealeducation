export type RegistrationsTrendRange = 7 | 30 | 90;

export interface CandidateRegistrationTrendPoint {
  date: string;
  count: number;
  graduated: number;
  dropped: number;
}
