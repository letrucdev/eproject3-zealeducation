export type DataTableAlign = 'left' | 'center' | 'right';

export interface DataTableColumn<T = unknown> {
  key: string;
  header: string;
  width?: string;
  minWidth?: string;
  align?: DataTableAlign;
  headerClass?: string;
  cellClass?: string;
  value?: (row: T) => string | number | null | undefined;
}
