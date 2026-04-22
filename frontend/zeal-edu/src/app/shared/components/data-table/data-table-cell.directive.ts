import { Directive, TemplateRef, inject, input } from '@angular/core';

export interface DataTableCellContext<T = unknown> {
  $implicit: T;
  row: T;
  index: number;
}

/**
 * Marks an `<ng-template>` as the renderer for a column (matched by `key`).
 *
 * Defaults to `T = any` so `let-row` in consumer templates is usable without
 * casts. For strict row typing, extend this directive with the row type pinned
 * and a custom selector — Angular will then infer `let-row` as T.
 *
 * @example
 * ```ts
 * // staff-cell.directive.ts
 * @Directive({
 *   selector: '[staffCell]',
 *   providers: [{ provide: DataTableCellDef, useExisting: StaffCellDef }],
 * })
 * export class StaffCellDef extends DataTableCellDef<StaffListItem> {
 *   override readonly appDataTableCell = input.required<string>({ alias: 'staffCell' });
 * }
 * ```
 *
 * ```html
 * <ng-template staffCell="fullName" let-row>
 *   {{ row.fullName }} <!-- row: StaffListItem -->
 * </ng-template>
 * ```
 */
@Directive({
  selector: '[appDataTableCell]',
})
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export class DataTableCellDef<T = any> {
  readonly appDataTableCell = input.required<string>();
  readonly template: TemplateRef<DataTableCellContext<T>> = inject(TemplateRef);

  // Non-generic guard so subclasses can redeclare with a concrete row type
  // without hitting static-side variance errors. When used directly (without a
  // typed subclass), `let-row` is `any`.
  static ngTemplateContextGuard(
    _dir: DataTableCellDef,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    ctx: unknown,
  ): ctx is DataTableCellContext<any> {
    return true;
  }
}
