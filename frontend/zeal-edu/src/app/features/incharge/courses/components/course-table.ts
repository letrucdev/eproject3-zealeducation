import { DatePipe } from '@angular/common';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { CourseListItem } from '@core/models/course-list-item';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '@shared/components/data-table';

@Directive({
  selector: '[courseCell]',
  providers: [{ provide: DataTableCellDef, useExisting: CourseCellDef }],
})
export class CourseCellDef extends DataTableCellDef<CourseListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'courseCell' });

  static override ngTemplateContextGuard(
    _dir: CourseCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<CourseListItem> {
    return true;
  }
}

@Component({
  selector: 'app-course-table',
  imports: [
    DataTable,
    CourseCellDef,
    VndPipe,
    DatePipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucidePencil })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'course-table.html',
})
export class CourseTable {
  readonly page = input<PaginatedList<CourseListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);

  readonly editClicked = output<CourseListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();

  protected readonly columns: DataTableColumn<CourseListItem>[] = [
    { key: 'courseName', header: 'Course Name', width: 'w-72' },
    { key: 'durationWeeks', header: 'Duration', width: 'w-32', align: 'center' },
    { key: 'baseFee', header: 'Base Fee', width: 'w-36', align: 'right' },
    { key: 'isActive', header: 'Status', width: 'w-32', align: 'center' },
    { key: 'updatedAt', header: 'Last Updated', width: 'w-40' },
    { key: 'actions', header: 'Actions', width: 'w-28', align: 'right' },
  ];

  protected readonly trackById = (row: CourseListItem): string => row.courseId;
}
