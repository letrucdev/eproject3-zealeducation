export interface AssetStatistics {
  total: number;          // active only: good + maintenance + faulty
  good: number;
  maintenance: number;
  faulty: number;
  decommissioned: number; // shown separately as a toggle card
}
