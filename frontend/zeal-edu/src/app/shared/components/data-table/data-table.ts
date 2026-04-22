import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  contentChildren,
  effect,
  input,
  output,
  signal,
  TemplateRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import { lucideChevronLeft, lucideChevronRight } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { PaginatedList } from '../../../core/models/paginated-list';
import { DigitsOnlyDirective } from '../../directives/digits-only.directive';
import { DataTableCellContext, DataTableCellDef } from './data-table-cell.directive';
import { DataTableAlign, DataTableColumn } from './data-table-column';

@Component({
  selector: 'app-data-table',
  imports: [
    NgTemplateOutlet,
    ReactiveFormsModule,
    HlmTableImports,
    HlmButtonImports,
    HlmIconImports,
    HlmInputImports,
    HlmSelectImports,
    HlmSkeletonImports,
    DigitsOnlyDirective,
  ],
  providers: [
    provideIcons({
      lucideChevronLeft,
      lucideChevronRight,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'data-table.html',
})
export class DataTable<T = unknown> {
  readonly columns = input.required<DataTableColumn<T>[]>();
  readonly rows = input<T[] | null | undefined>([]);
  readonly page = input<PaginatedList<T> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly emptyMessage = input<string>('No data found.');
  readonly rowHeight = input<string | null>(null);
  readonly trackBy = input<((row: T, index: number) => unknown) | null>(null);

  readonly pageSize = input<number>(10);
  readonly pageSizeOptions = input<number[]>([10, 20, 50]);
  readonly itemLabel = input<string>('items');

  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();

  protected readonly cellDefs = contentChildren<DataTableCellDef<T>>(DataTableCellDef);
  protected readonly skeletonRows = [0, 1, 2, 3, 4];
  protected readonly pageSizeControl = new FormControl<number>(10, { nonNullable: true });
  protected readonly pageInputValue = signal<string>('1');

  readonly displayRows = computed<T[]>(() => this.page()?.items ?? this.rows() ?? []);

  readonly trackFn = computed<(row: T, index: number) => unknown>(
    () => this.trackBy() ?? ((_row, index) => index),
  );

  readonly showingFrom = computed(() => {
    const pg = this.page();
    if (!pg || pg.totalCount === 0) return 0;
    return (pg.pageNumber - 1) * this.pageSize() + 1;
  });

  readonly showingTo = computed(() => {
    const pg = this.page();
    if (!pg) return 0;
    return Math.min(pg.pageNumber * this.pageSize(), pg.totalCount);
  });

  constructor() {
    effect(() => {
      const size = this.pageSize();
      if (this.pageSizeControl.value !== size) {
        this.pageSizeControl.setValue(size, { emitEvent: false });
      }
    });

    effect(() => {
      const pg = this.page();
      if (pg) this.pageInputValue.set(String(pg.pageNumber));
    });

    this.pageSizeControl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((size) => this.pageSizeChanged.emit(size));
  }

  protected cellTemplateFor(key: string): TemplateRef<DataTableCellContext<T>> | null {
    return this.cellDefs().find((def) => def.appDataTableCell() === key)?.template ?? null;
  }

  protected headerClasses(col: DataTableColumn<T>): string {
    const parts: string[] = [];
    if (col.width && this._isTailwindClass(col.width)) parts.push(col.width);
    parts.push(this._alignClass(col.align));
    if (col.headerClass) parts.push(col.headerClass);
    return parts.join(' ');
  }

  protected cellClasses(col: DataTableColumn<T>): string {
    const parts: string[] = [this._alignClass(col.align)];
    if (col.cellClass) parts.push(col.cellClass);
    return parts.join(' ');
  }

  protected widthStyle(col: DataTableColumn<T>): string | null {
    if (!col.width || this._isTailwindClass(col.width)) return null;
    return col.width;
  }

  protected minWidthStyle(col: DataTableColumn<T>): string | null {
    if (!col.minWidth || this._isTailwindClass(col.minWidth)) return null;
    return col.minWidth;
  }

  protected renderValue(row: T, col: DataTableColumn<T>): string {
    if (col.value) {
      const v = col.value(row);
      return v == null ? '' : String(v);
    }
    const record = row as Record<string, unknown>;
    const v = record[col.key];
    return v == null ? '' : String(v);
  }

  protected prev(): void {
    const current = this.page()?.pageNumber ?? 1;
    if (current > 1) this.pageChanged.emit(current - 1);
  }

  protected next(): void {
    const pg = this.page();
    if (pg?.hasNextPage) this.pageChanged.emit(pg.pageNumber + 1);
  }

  protected commitPageInput(): void {
    const pg = this.page();
    if (!pg) return;

    const raw = this.pageInputValue();
    const value = parseInt(raw, 10);

    if (isNaN(value) || value < 1 || value > pg.totalPages) {
      this.pageInputValue.set(String(pg.pageNumber));
      return;
    }

    if (value === pg.pageNumber) {
      this.pageInputValue.set(String(pg.pageNumber));
      return;
    }

    this.pageChanged.emit(value);
  }

  private _alignClass(align?: DataTableAlign): string {
    switch (align) {
      case 'center':
        return 'text-center';
      case 'right':
        return 'text-right';
      case 'left':
      default:
        return 'text-left';
    }
  }

  private _isTailwindClass(value: string): boolean {
    return /^(w-|min-w-|max-w-)/.test(value);
  }
}
