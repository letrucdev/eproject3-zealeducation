import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { toast } from '@spartan-ng/brain/sonner';
import { StaffListItem } from '@core/models/staff-list-item';
import { UserRole } from '@core/models/user-role';
import {
  StaffFilterBar,
  StaffFilterValue,
} from './components/staff-filter-bar';
import {
  StaffFormDialog,
  StaffFormSubmit,
} from './components/staff-form-dialog';
import { StaffStatsCards } from './components/staff-stats-cards';
import { StaffTable } from './components/staff-table';
import { StaffListQuery } from './models/staff-form-payload';
import { StaffAccountsService } from './staff-accounts.service';

@Component({
  selector: 'app-staff-accounts-page',
  imports: [
    StaffStatsCards,
    StaffFilterBar,
    StaffTable,
    StaffFormDialog,
  ],
  providers: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">Staff Account Management</h1>
          <p class="text-muted-foreground mt-1 max-w-2xl text-sm">
            Create, update, and deactivate staff accounts, assign roles, and monitor
            account activity across the institution from a single workspace.
          </p>
        </div>
      </header>

      <app-staff-stats-cards
        [stats]="statsQuery.data()"
        [isLoading]="statsQuery.isPending()"
      />

      <app-staff-filter-bar
        [initial]="initialFilter"
        (filterChanged)="onFilterChanged($event)"
        (createClicked)="onCreateClicked()"
      />

      <app-staff-table
        [page]="listQuery.data()"
        [isLoading]="listQuery.isPending()"
        [pageSize]="pageSize()"
        (editClicked)="onEditClicked($event)"
        (pageChanged)="onPageChanged($event)"
        (pageSizeChanged)="onPageSizeChanged($event)"
      />

      <app-staff-form-dialog
        #formDialog
        [submitting]="isSubmittingForm()"
        (submitted)="onFormSubmitted($event)"
      />
    </section>
  `,
})
export default class StaffAccountsPage {
  private readonly _service = inject(StaffAccountsService);

  protected readonly formDialog = viewChild.required<StaffFormDialog>('formDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly roleFilter = signal<UserRole | ''>('');
  protected readonly statusFilter = signal<'all' | 'active' | 'inactive'>('all');

  protected readonly initialFilter: StaffFilterValue = {
    search: '',
    role: '',
    status: 'all',
  };

  protected readonly editingStaffId = signal<string | null>(null);

  private readonly _listParams = computed<StaffListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    role: this.roleFilter() || undefined,
    isActive:
      this.statusFilter() === 'all'
        ? undefined
        : this.statusFilter() === 'active',
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly statsQuery = this._service.statisticsQuery();
  protected readonly detailQuery = this._service.detailQuery(this.editingStaffId);
  protected readonly createMutation = this._service.createMutation();
  protected readonly updateMutation = this._service.updateMutation();
  protected readonly createFacultyMutation = this._service.createFacultyMutation();
  protected readonly updateFacultyMutation = this._service.updateFacultyMutation();

  protected readonly isSubmittingForm = computed(
    () =>
      this.createMutation.isPending() ||
      this.updateMutation.isPending() ||
      this.createFacultyMutation.isPending() ||
      this.updateFacultyMutation.isPending(),
  );

  constructor() {
    effect(() => {
      const detail = this.detailQuery.data();
      const currentId = this.editingStaffId();
      if (detail && currentId === detail.staffId) {
        untracked(() => {
          this.formDialog().openEdit(detail);
          this.editingStaffId.set(null);
        });
      }
    });

    effect(() => {
      const error = this.detailQuery.error();
      if (error) {
        untracked(() => {
          toast.error(this._extractError(error) ?? 'Failed to load staff details.');
          this.editingStaffId.set(null);
        });
      }
    });
  }

  onFilterChanged(value: StaffFilterValue): void {
    this.search.set(value.search);
    this.roleFilter.set(value.role);
    this.statusFilter.set(value.status);
    this.page.set(1);
  }

  onPageChanged(page: number): void {
    this.page.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  onCreateClicked(): void {
    this.formDialog().openCreate();
  }

  onEditClicked(staff: StaffListItem): void {
    this.editingStaffId.set(staff.staffId);
  }

  onFormSubmitted(event: StaffFormSubmit): void {
    if (event.kind === 'faculty' && event.mode === 'create') {
      this.createFacultyMutation.mutate(event.payload, {
        onSuccess: () => {
          toast.success('Faculty account created successfully.');
          this.formDialog().close();
        },
        onError: (err) => toast.error(this._extractError(err) ?? 'Failed to create faculty.'),
      });
      return;
    }

    if (event.kind === 'faculty' && event.mode === 'edit') {
      this.updateFacultyMutation.mutate(
        { staffId: event.staffId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Faculty account updated successfully.');
            this.formDialog().close();
          },
          onError: (err) => toast.error(this._extractError(err) ?? 'Failed to update faculty.'),
        },
      );
      return;
    }

    if (event.mode === 'create') {
      this.createMutation.mutate(event.payload, {
        onSuccess: () => {
          toast.success('Staff account created successfully.');
          this.formDialog().close();
        },
        onError: (err) => toast.error(this._extractError(err) ?? 'Failed to create staff.'),
      });
    } else {
      this.updateMutation.mutate(
        { staffId: event.staffId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Staff account updated successfully.');
            this.formDialog().close();
          },
          onError: (err) => toast.error(this._extractError(err) ?? 'Failed to update staff.'),
        },
      );
    }
  }

  private _extractError(err: HttpErrorResponse): string | undefined {
    const body = err.error as { message?: string } | string | null | undefined;
    if (!body) return undefined;
    if (typeof body === 'string') return body.trim() || undefined;
    return body.message?.trim();
  }
}
