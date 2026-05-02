# DataTable — reusable table

Generic, paginated, column-driven table. Pass `columns` + data (`rows` or `page`), render custom cells via template directives.

Import from `src/app/shared/components/data-table`.

## Inputs

| Input | Type | Notes |
|---|---|---|
| `columns` | `DataTableColumn<T>[]` | Required. See "Column config". |
| `rows` | `T[]` | Non-paginated data. Ignored if `page` is supplied. |
| `page` | `PaginatedList<T>` | Paginated data. When set, the pagination footer renders. |
| `isLoading` | `boolean` | Shows 5 skeleton rows. |
| `emptyMessage` | `string` | Text shown when rows are empty. Default: `"No data found."` |
| `rowHeight` | `string` | CSS height applied to each `<tr>` (e.g. `'56px'`). |
| `trackBy` | `(row, index) => unknown` | Defaults to `index`. |
| `pageSize` | `number` | Default `10`. |
| `pageSizeOptions` | `number[]` | Default `[10, 20, 50]`. |
| `itemLabel` | `string` | Footer label (`"Showing 1 to 10 of 23 <itemLabel>"`). Default `"items"`. |
| `sortBy` | `string \| null` | Currently sorted column key (matches `column.sortKey ?? column.key`). |
| `sortDirection` | `'asc' \| 'desc' \| null` | Direction for the active sort column. |

## Outputs

| Output | Payload |
|---|---|
| `pageChanged` | `number` (new page) |
| `pageSizeChanged` | `number` (new size) |
| `sortChanged` | `{ sortBy: string; sortDirection: 'asc' \| 'desc' }` |

Row actions (edit, delete, etc.) are emitted by the **consumer** from inside the cell template — don't add new outputs to `DataTable` for them.

## Column config (`DataTableColumn<T>`)

```ts
{
  key: 'fullName',           // must match the cell template's key
  header: 'Name',
  width: 'w-64',             // Tailwind utility OR CSS length ('220px', '15rem')
  minWidth: 'w-32',          // optional
  align: 'center',           // 'left' | 'center' | 'right' — applied to th + td
  headerClass: 'bg-muted',   // optional extra classes on <th>
  cellClass: 'font-medium',  // optional extra classes on <td>
  value: (row) => row.email, // fallback when no cell template is provided
  sortable: true,            // turn the header into a click-to-sort button
  sortKey: 'fullName',       // optional override sent in `sortChanged`; defaults to `key`
}
```

- If `width` starts with `w-`, `min-w-`, `max-w-` it is treated as a Tailwind class; otherwise it's set as an inline `style.width`.
- If a column has no matching `<ng-template>`, `value(row)` or `row[key]` is rendered as text.

## Cell templates

Two patterns — pick one per consumer.

### A. Untyped (quick) — `appDataTableCell`

Use when type safety isn't important (throwaway tables, prototypes). `let-row` is `any`.

```ts
import { DataTable, DataTableCellDef } from '.../shared/components/data-table';

@Component({
  imports: [DataTable, DataTableCellDef, /* ... */],
  template: `
    <app-data-table [columns]="columns" [rows]="rows()">
      <ng-template appDataTableCell="name" let-row>
        {{ row.name }}
      </ng-template>
    </app-data-table>
  `,
})
```

### B. Typed (recommended) — typed subclass directive

Define one small directive per row type. Template gets strict IntelliSense and the compiler catches misspelled properties.

```ts
@Directive({
  selector: '[staffCell]',
  providers: [{ provide: DataTableCellDef, useExisting: StaffCellDef }],
})
export class StaffCellDef extends DataTableCellDef<StaffListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'staffCell' });

  static override ngTemplateContextGuard(
    _dir: StaffCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<StaffListItem> {
    return true;
  }
}
```

Three required pieces:
1. `extends DataTableCellDef<RowType>` — pins the generic.
2. `providers: [{ provide: DataTableCellDef, useExisting: <this> }]` — makes `contentChildren(DataTableCellDef)` inside `DataTable` find this instance.
3. `static override ngTemplateContextGuard(...)` — pins `let-row` to `RowType` for the Angular Language Service (do **not** rely on the base class's inherited guard — it resolves to `any` in templates).

Usage:

```html
<ng-template staffCell="fullName" let-row>
  <!-- row is StaffListItem -->
  {{ row.fullName }}
</ng-template>
```

Put the directive in the same file as the table component (or a sibling `*-cell.directive.ts` if shared) and include it in the component's `imports`.

## Pagination

Pagination is controlled externally — wire `page` + page-size signals to a query.

```ts
protected readonly page = signal(1);
protected readonly pageSize = signal(10);

protected readonly query = this.service.listQuery(computed(() => ({
  page: this.page(),
  pageSize: this.pageSize(),
})));

// template
<app-data-table
  [page]="query.data()"
  [isLoading]="query.isPending()"
  [pageSize]="pageSize()"
  (pageChanged)="page.set($event)"
  (pageSizeChanged)="pageSize.set($event); page.set(1)"
/>
```

Reset `page` to `1` when `pageSize` changes.

## Sorting

Server-side, controlled externally — the table only emits the user's intent.

- Mark a column with `sortable: true`. Its header becomes a button with up/down/up-down arrow icons.
- Pass the active sort via `[sortBy]` and `[sortDirection]`.
- `(sortChanged)` emits `{ sortBy, sortDirection }` when the user clicks. Clicking the active column toggles asc → desc; clicking a different column resets to asc.
- Use `sortKey` when the column key (template/cell key) differs from the field name the API expects.

```ts
protected readonly sortBy = signal<string | null>('createdAt');
protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

onSortChanged({ sortBy, sortDirection }: DataTableSortChange): void {
  this.sortBy.set(sortBy);
  this.sortDirection.set(sortDirection);
  this.page.set(1); // reset to first page on sort change
}
```

```html
<app-data-table
  [columns]="columns"
  [page]="query.data()"
  [sortBy]="sortBy()"
  [sortDirection]="sortDirection()"
  (sortChanged)="onSortChanged($event)"
/>
```

## Gotchas

- **IDE not showing types after adding a typed cell def**: restart the TS server (Command Palette → *TypeScript: Restart TS Server*). Template type-check state is cached across directive changes.
- **Column key ↔ template key mismatch**: silent fallback to `value()` / `row[key]`. Double-check both sides when a cell unexpectedly renders text.
- **`contentChildren` doesn't see your typed cell def**: you forgot the `useExisting` provider, or the directive isn't in the component's `imports`.
- **Don't add row-action outputs to `DataTable`**: emit from the consumer component via the cell template's `(click)` — it has the row in scope and the correct type.
- **Width as raw CSS**: `'220px'` goes to inline `style.width`, not a class. If you need both a class and an inline width, use the Tailwind arbitrary syntax: `'w-[220px]'`.

## Reference: see `staff-table.ts`

[features/system-admin/staff-accounts/components/staff-table.ts](../../../features/system-admin/staff-accounts/components/staff-table.ts) is the canonical example — typed cell def + paginated `page` + edit action emitted from the template.
