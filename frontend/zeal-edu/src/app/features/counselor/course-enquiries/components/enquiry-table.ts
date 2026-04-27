import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideEye, lucidePencil, lucideUserCheck } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { CourseEnquiryListItem } from '@core/models/course-enquiry-list-item';
import { EnquiryStatus } from '@core/models/enquiry-status';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { PaginatedList } from '@core/models/paginated-list';
import { ENQUIRY_SOURCE_LABELS, ENQUIRY_STATUS_LABELS } from './enquiry-labels';
import { EnquirySource } from '@core/models/enquiry-source';

@Directive({
  selector: '[enquiryCell]',
  providers: [{ provide: DataTableCellDef, useExisting: EnquiryCellDef }],
})
export class EnquiryCellDef extends DataTableCellDef<CourseEnquiryListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'enquiryCell' });

  static override ngTemplateContextGuard(
    _dir: EnquiryCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<CourseEnquiryListItem> {
    return true;
  }
}

@Component({
  selector: 'app-enquiry-table',
  imports: [DataTable, EnquiryCellDef, DatePipe, HlmBadgeImports, HlmButtonImports, HlmIconImports],
  providers: [provideIcons({ lucidePencil, lucideEye, lucideUserCheck })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-table.html',
})
export class EnquiryTable {
  readonly page = input<PaginatedList<CourseEnquiryListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly viewClicked = output<CourseEnquiryListItem>();
  readonly editClicked = output<CourseEnquiryListItem>();
  readonly convertClicked = output<CourseEnquiryListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly statuses = EnquiryStatus;

  protected readonly columns: DataTableColumn<CourseEnquiryListItem>[] = [
    { key: 'fullName', header: 'Name', width: 'w-56' },
    { key: 'phone', header: 'Phone', width: 'w-36' },
    { key: 'email', header: 'Email', width: 'w-56' },
    { key: 'courseInterestedName', header: 'Course', width: 'w-48' },
    { key: 'source', header: 'Source', sortable: true, width: 'w-32' },
    { key: 'status', header: 'Status', sortable: true, width: 'w-36', align: 'center' },
    { key: 'nextFollowUpDate', header: 'Next Follow-Up', sortable: true, width: 'w-40' },
    { key: 'assignedCounselorName', header: 'Counselor', width: 'w-40' },
    { key: 'actions', header: 'Actions', width: 'w-48', align: 'right' },
  ];

  protected readonly trackById = (row: CourseEnquiryListItem): string => row.enquiryId;

  protected statusLabel(status: EnquiryStatus): string {
    return ENQUIRY_STATUS_LABELS[status];
  }

  protected sourceLabel(source: EnquirySource): string {
    return ENQUIRY_SOURCE_LABELS[source];
  }

  protected isOverdue(row: CourseEnquiryListItem): boolean {
    if (!row.nextFollowUpDate) return false;
    if (
      row.status === EnquiryStatus.Converted ||
      row.status === EnquiryStatus.Closed
    )
      return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const due = new Date(row.nextFollowUpDate);
    due.setHours(0, 0, 0, 0);
    return due.getTime() <= today.getTime();
  }
}
