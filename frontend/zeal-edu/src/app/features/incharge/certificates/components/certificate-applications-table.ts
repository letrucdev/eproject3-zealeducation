import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCheckCheck,
  lucideCircleCheck,
  lucideCircleX,
  lucideRefreshCw,
} from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';
import { CertificateApplicationListItem } from '../models/certificate-application';

@Directive({
  selector: '[certificateCell]',
  providers: [{ provide: DataTableCellDef, useExisting: CertificateCellDef }],
})
export class CertificateCellDef extends DataTableCellDef<CertificateApplicationListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'certificateCell' });

  static override ngTemplateContextGuard(
    _dir: CertificateCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<CertificateApplicationListItem> {
    return true;
  }
}

@Component({
  selector: 'app-certificate-applications-table',
  imports: [
    DatePipe,
    DataTable,
    CertificateCellDef,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSpinnerImports,
  ],
  providers: [
    provideIcons({ lucideCheckCheck, lucideCircleCheck, lucideCircleX, lucideRefreshCw }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'certificate-applications-table.html',
})
export class CertificateApplicationsTable {
  readonly page = input<PaginatedList<CertificateApplicationListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);
  readonly approvingId = input<string | null>(null);
  readonly regeneratingId = input<string | null>(null);

  readonly approveClicked = output<CertificateApplicationListItem>();
  readonly regenerateClicked = output<CertificateApplicationListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly columns: DataTableColumn<CertificateApplicationListItem>[] = [
    { key: 'createdAt', header: 'Applied at', sortable: true, sortKey: 'createdAt', width: 'w-40' },
    {
      key: 'candidate',
      header: 'Candidate',
      sortable: true,
      sortKey: 'candidateName',
      width: 'w-52',
    },
    { key: 'course', header: 'Course / Batch', sortable: true, sortKey: 'courseName' },
    { key: 'fees', header: 'Fees', width: 'w-20', align: 'center' },
    { key: 'attendance', header: 'Attendance', align: 'center' },
    { key: 'exams', header: 'Exams', width: 'w-20', align: 'center' },
    { key: 'status', header: 'Status', sortable: true, sortKey: 'status', align: 'center' },
    { key: 'certificateNumber', header: 'Certificate No.' },
    { key: 'approval', header: 'Approval', sortable: true, sortKey: 'approvedAt' },
    { key: 'actions', header: 'Actions', align: 'right' },
  ];

  protected readonly trackById = (row: CertificateApplicationListItem): string => row.applicationId;
}
