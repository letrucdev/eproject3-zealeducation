import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { ROLE_LABELS } from '../../../../core/layout/nav-items';
import { Gender } from '../../../../core/models/gender';
import { PaginatedList } from '../../../../core/models/paginated-list';
import { StaffListItem } from '../../../../core/models/staff-list-item';
import { UserRole } from '../../../../core/models/user-role';
import { getUserInitials } from '../../../../core/utils/user-initials';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '../../../../shared/components/data-table';

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

@Component({
  selector: 'app-staff-table',
  imports: [DataTable, StaffCellDef, DatePipe, HlmBadgeImports, HlmButtonImports, HlmIconImports],
  providers: [provideIcons({ lucidePencil })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'staff-table.html',
})
export class StaffTable {
  readonly page = input<PaginatedList<StaffListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);

  readonly editClicked = output<StaffListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();

  protected readonly getUserInitials = getUserInitials;

  protected readonly columns: DataTableColumn<StaffListItem>[] = [
    { key: 'username', header: 'Username', width: 'w-36' },
    { key: 'fullName', header: 'Name', width: 'w-64' },
    { key: 'email', header: 'Email', width: 'w-64' },
    { key: 'phone', header: 'Phone', width: 'w-36' },
    { key: 'gender', header: 'Gender', width: 'w-24' },
    { key: 'dob', header: 'Date of birth', width: 'w-36' },
    { key: 'department', header: 'Department', width: 'w-40' },
    { key: 'position', header: 'Position', width: 'w-40' },
    { key: 'role', header: 'Role', width: 'w-32', align: 'left' },
    { key: 'status', header: 'Status', width: 'w-32', align: 'center' },
    { key: 'actions', header: 'Actions', width: 'w-40', align: 'right' },
  ];

  protected readonly trackById = (row: StaffListItem): string => row.staffId;

  protected roleLabel(role: UserRole): string {
    return ROLE_LABELS[role] ?? role;
  }

  protected genderLabel(gender: Gender): string {
    switch (gender) {
      case Gender.Male:
        return 'Male';
      case Gender.Female:
        return 'Female';
      case Gender.Other:
        return 'Other';
      default:
        return gender;
    }
  }
}
