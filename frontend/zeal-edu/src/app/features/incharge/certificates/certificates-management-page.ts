import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { DataTableSortChange } from '@shared/components/data-table';
import { CertificateApplicationsService } from './certificate-applications.service';
import { ApproveCertificateDialog } from './components/approve-certificate-dialog';
import { CertificateApplicationsTable } from './components/certificate-applications-table';
import {
  CertificateApplicationListItem,
  CertificateApplicationListQuery,
  CertificateApplicationStatus,
} from './models/certificate-application';

@Component({
  selector: 'app-certificates-management-page',
  imports: [
    FormsModule,
    HlmCardImports,
    HlmInputImports,
    HlmSelectImports,
    CertificateApplicationsTable,
    ApproveCertificateDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 class="text-2xl font-semibold tracking-tight">Certificate Applications</h1>
          <p class="text-muted-foreground mt-1 max-w-2xl text-sm">
            Review candidate applications and issue certificates. A certificate can only be approved
            when the candidate has fully paid the course fees, attended at least 80% of sessions,
            and passed every finalized exam.
          </p>
        </div>
      </header>

      <section hlmCard>
        <div hlmCardHeader>
          <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <input
              hlmInput
              type="search"
              placeholder="Search by candidate, code or course..."
              class="sm:max-w-sm"
              [ngModel]="search()"
              (ngModelChange)="onSearchChanged($event)"
            />

            <hlm-select
              [ngModel]="statusFilter() ?? 'All'"
              (ngModelChange)="onStatusFilterChanged($event)"
              class="w-full sm:w-44"
            >
              <hlm-select-trigger>
                <hlm-select-value placeholder="Status" />
              </hlm-select-trigger>
              <hlm-select-content *hlmSelectPortal>
                <hlm-select-item value="All">All</hlm-select-item>
                <hlm-select-item value="Pending">Pending</hlm-select-item>
                <hlm-select-item value="Approved">Approved</hlm-select-item>
              </hlm-select-content>
            </hlm-select>
          </div>
        </div>
        <div hlmCardContent>
          <app-certificate-applications-table
            [page]="listQuery.data()"
            [isLoading]="listQuery.isPending()"
            [pageSize]="pageSize()"
            [sortBy]="sortBy()"
            [sortDirection]="sortDirection()"
            [approvingId]="approvingId()"
            [regeneratingId]="regeneratingId()"
            (approveClicked)="onApproveClicked($event)"
            (regenerateClicked)="onRegenerateClicked($event)"
            (pageChanged)="onPageChanged($event)"
            (pageSizeChanged)="onPageSizeChanged($event)"
            (sortChanged)="onSortChanged($event)"
          />
        </div>
      </section>

      <app-approve-certificate-dialog
        #approveDialog
        [submitting]="approveMutation.isPending()"
        (confirmed)="onApproveConfirmed($event)"
      />
    </section>
  `,
})
export default class CertificatesManagementPage {
  private readonly _service = inject(CertificateApplicationsService);

  protected readonly approveDialog = viewChild<ApproveCertificateDialog>('approveDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<CertificateApplicationStatus | null>('Pending');
  protected readonly sortBy = signal<string | null>('createdAt');
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');
  protected readonly approvingId = signal<string | null>(null);
  protected readonly regeneratingId = signal<string | null>(null);

  private readonly _params = computed<CertificateApplicationListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    status: this.statusFilter(),
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.listQuery(this._params);
  protected readonly approveMutation = this._service.approveMutation();
  protected readonly regenerateMutation = this._service.regenerateMutation();

  protected onSearchChanged(value: string): void {
    this.search.set(value ?? '');
    this.page.set(1);
  }

  protected onStatusFilterChanged(value: string): void {
    if (value === 'Pending' || value === 'Approved') {
      this.statusFilter.set(value);
    } else {
      this.statusFilter.set(null);
    }
    this.page.set(1);
  }

  protected onPageChanged(page: number): void {
    this.page.set(page);
  }

  protected onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  protected onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }

  protected onApproveClicked(row: CertificateApplicationListItem): void {
    this.approveDialog()?.open(row);
  }

  protected onApproveConfirmed(row: CertificateApplicationListItem): void {
    if (this.approvingId() === row.applicationId) return;
    this.approvingId.set(row.applicationId);
    this.approveMutation.mutate(
      { applicationId: row.applicationId },
      {
        onSuccess: (data) => {
          toast.success(`Certificate ${data.certificateNumber} issued.`);
          this.approveDialog()?.close();
        },
        onSettled: () => {
          this.approvingId.set(null);
        },
      },
    );
  }

  protected onRegenerateClicked(row: CertificateApplicationListItem): void {
    if (this.regeneratingId() === row.applicationId) return;
    this.regeneratingId.set(row.applicationId);
    this.regenerateMutation.mutate(
      { applicationId: row.applicationId },
      {
        onSuccess: (data) => {
          toast.success(`Certificate ${data.certificateNumber} regenerated.`);
        },
        onSettled: () => {
          this.regeneratingId.set(null);
        },
      },
    );
  }
}
