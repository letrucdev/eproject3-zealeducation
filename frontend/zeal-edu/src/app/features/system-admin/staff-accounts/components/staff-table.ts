import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideChevronLeft, lucideChevronRight, lucidePencil } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { HlmTableImports } from '@spartan-ng/helm/table';
import { ROLE_LABELS } from '../../../../core/layout/nav-items';
import { Gender } from '../../../../core/models/gender';
import { PaginatedList } from '../../../../core/models/paginated-list';
import { StaffListItem } from '../../../../core/models/staff-list-item';
import { UserRole } from '../../../../core/models/user-role';
import { getUserInitials } from '../../../../core/utils/user-initials';

@Component({
  selector: 'app-staff-table',
  imports: [
    DatePipe,
    HlmTableImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
  ],
  providers: [
    provideIcons({
      lucidePencil,
      lucideChevronLeft,
      lucideChevronRight,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="overflow-hidden rounded-md border w-full">
      <div hlmTableContainer>
        <table hlmTable class="w-full">
          <thead hlmTHead>
            <tr hlmTr>
              <th hlmTh class="w-36 text-left">Username</th>
              <th hlmTh class="w-64 text-left">Name</th>
              <th hlmTh class="w-64 text-left">Email</th>
              <th hlmTh class="w-36 text-left">Phone</th>
              <th hlmTh class="w-24 text-left">Gender</th>
              <th hlmTh class="w-36 text-left">Date of birth</th>
              <th hlmTh class="w-40 text-left">Department</th>
              <th hlmTh class="w-40 text-left">Position</th>
              <th hlmTh class="w-32">Role</th>
              <th hlmTh class="w-32">Status</th>
              <th hlmTh class="w-40 text-right">Actions</th>
            </tr>
          </thead>
          <tbody hlmTBody>
            @if (isLoading()) {
              @for (row of skeletonRows; track row) {
                <tr hlmTr>
                  <td hlmTd colspan="11">
                    <hlm-skeleton class="h-14 w-full" />
                  </td>
                </tr>
              }
            } @else if (rows().length === 0) {
              <tr hlmTr>
                <td hlmTd colspan="11" class="h-14 text-center">No staff found.</td>
              </tr>
            } @else {
              @for (row of rows(); track row.staffId) {
                <tr hlmTr>
                  <td hlmTd class="font-mono text-xs font-semibold">
                    {{ row.username }}
                  </td>
                  <td hlmTd>
                    <div class="flex items-center gap-3">
                      <span
                        class="bg-muted flex size-8 shrink-0 items-center justify-center rounded-full text-xs font-semibold"
                        aria-hidden="true"
                      >
                        {{ getUserInitials(row.fullName) }}
                      </span>
                      <span class="font-medium">{{ row.fullName }}</span>
                    </div>
                  </td>
                  <td hlmTd>{{ row.email }}</td>
                  <td hlmTd>{{ row.phone }}</td>
                  <td hlmTd>{{ genderLabel(row.gender) }}</td>
                  <td hlmTd>
                    {{ row.dob | date: 'dd MMM yyyy' }}
                  </td>
                  <td hlmTd>{{ row.department }}</td>
                  <td hlmTd>{{ row.position }}</td>
                  <td hlmTd>
                    <span hlmBadge variant="secondary">{{ roleLabel(row.role) }}</span>
                  </td>
                  <td hlmTd>
                    @if (row.isActive) {
                      <span hlmBadge class="bg-emerald-100 text-emerald-800">
                        <span class="me-1 size-1.5 rounded-full bg-emerald-600"></span>
                        Active
                      </span>
                    } @else {
                      <span hlmBadge variant="secondary">
                        <span class="me-1 size-1.5 rounded-full bg-slate-400"></span>
                        Inactive
                      </span>
                    }
                  </td>
                  <td hlmTd>
                    <div class="flex items-center justify-end gap-1">
                      <button
                        hlmBtn
                        variant="ghost"
                        size="sm"
                        type="button"
                        (click)="editClicked.emit(row)"
                        [attr.aria-label]="'Edit ' + row.fullName"
                      >
                        <ng-icon hlm name="lucidePencil" size="sm" />
                        Edit
                      </button>
                    </div>
                  </td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>
    </div>

    <div class="flex flex-col justify-between py-4 sm:flex-row sm:items-center">
      <div class="text-muted-foreground text-sm">
        @if (page(); as pg) {
          Showing {{ showingFrom() }} to {{ showingTo() }} of {{ pg.totalCount }} staff
        }
      </div>
      <div class="mt-2 flex items-center space-x-2 sm:mt-0">
        <button
          size="sm"
          variant="outline"
          hlmBtn
          type="button"
          [disabled]="!page()?.hasPreviousPage || isLoading()"
          (click)="prev()"
        >
          <ng-icon hlm name="lucideChevronLeft" size="sm" class="mr-1" />
          Previous
        </button>
        <button
          size="sm"
          variant="outline"
          hlmBtn
          type="button"
          [disabled]="!page()?.hasNextPage || isLoading()"
          (click)="next()"
        >
          Next
          <ng-icon hlm name="lucideChevronRight" size="sm" class="ml-1" />
        </button>
      </div>
    </div>
  `,
})
export class StaffTable {
  readonly page = input<PaginatedList<StaffListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);

  readonly editClicked = output<StaffListItem>();
  readonly pageChanged = output<number>();

  protected readonly skeletonRows = [0, 1, 2, 3, 4];
  protected readonly getUserInitials = getUserInitials;

  readonly rows = computed(() => this.page()?.items ?? []);

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

  protected prev(): void {
    const current = this.page()?.pageNumber ?? 1;
    if (current > 1) this.pageChanged.emit(current - 1);
  }

  protected next(): void {
    const pg = this.page();
    if (pg?.hasNextPage) this.pageChanged.emit(pg.pageNumber + 1);
  }
}
