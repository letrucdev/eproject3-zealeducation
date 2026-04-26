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
import { MaintenanceService } from './maintenance.service';
import { SystemAssetListItem } from '../../../core/models/system-asset-list-item';
import { AssetConditionStatus } from '../../../core/models/asset-condition-status';
import { AssetStatistics } from '../../../core/models/asset-statistics';
import { PaginatedList } from '../../../core/models/paginated-list';
import AssetStatsCards from './components/asset-stats-cards';
import AssetFilterBar, { AssetFilterValue } from './components/asset-filter-bar';
import AssetTable from './components/asset-table';
import AssetFormDialog, { AssetFormSubmit } from './components/asset-form-dialog';
import AssetConditionDialog, { AssetConditionSubmit } from './components/asset-condition-dialog';
import AssetDetailDialog from './components/asset-detail-dialog';

@Component({
  selector: 'app-maintenance-page',
  imports: [
    AssetStatsCards,
    AssetFilterBar,
    AssetTable,
    AssetFormDialog,
    AssetConditionDialog,
    AssetDetailDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'maintenance-page.html',
})
export default class MaintenancePage {
  private readonly _service = inject(MaintenanceService);

  protected readonly formDialog = viewChild.required<AssetFormDialog>('formDialog');
  protected readonly conditionDialog = viewChild.required<AssetConditionDialog>('conditionDialog');
  protected readonly detailDialog = viewChild.required<AssetDetailDialog>('detailDialog');

  // ── Filter state ────────────────────────────────────────────────────────────
  protected readonly search = signal('');
  protected readonly statusFilter = signal<AssetConditionStatus | ''>('');
  protected readonly editingId = signal<string | null>(null);

  // ── View toggle: active assets vs decommissioned list ───────────────────────
  /** When true the table shows only Decommissioned assets; action buttons hidden. */
  protected readonly showingDecommissioned = signal(false);

  // ── Pagination ───────────────────────────────────────────────────────────────
  protected readonly pageSize = signal(10);
  protected readonly currentPage = signal(1);

  protected readonly initialFilter: AssetFilterValue = { search: '', conditionStatus: '' };

  // ── Queries / mutations ──────────────────────────────────────────────────────
  protected readonly listQuery = this._service.listQuery();
  protected readonly detailQuery = this._service.detailQuery(this.editingId);
  protected readonly createMutation = this._service.createMutation();
  protected readonly updateMutation = this._service.updateMutation();
  protected readonly updateConditionMutation = this._service.updateConditionMutation();
  protected readonly deleteMutation = this._service.deleteMutation();

  // ── Statistics (computed from flat list) ─────────────────────────────────────
  protected readonly stats = computed<AssetStatistics | null>(() => {
    const items = this.listQuery.data();
    if (!items) return null;
    const good = items.filter((i) => i.conditionStatus === AssetConditionStatus.Good).length;
    const maintenance = items.filter(
      (i) => i.conditionStatus === AssetConditionStatus.Maintenance,
    ).length;
    const faulty = items.filter((i) => i.conditionStatus === AssetConditionStatus.Faulty).length;
    const decommissioned = items.filter(
      (i) => i.conditionStatus === AssetConditionStatus.Decommissioned,
    ).length;
    return { total: good + maintenance + faulty, good, maintenance, faulty, decommissioned };
  });

  // ── Filtered list (respects active vs decommissioned view) ──────────────────
  protected readonly filteredItems = computed<SystemAssetListItem[]>(() => {
    const items = this.listQuery.data() ?? [];
    const search = this.search().toLowerCase().trim();
    const status = this.statusFilter();

    return items.filter((item) => {
      // View gate: show only the correct "bucket"
      if (this.showingDecommissioned()) {
        if (item.conditionStatus !== AssetConditionStatus.Decommissioned) return false;
      } else {
        if (item.conditionStatus === AssetConditionStatus.Decommissioned) return false;
      }

      const matchSearch =
        !search ||
        item.assetName.toLowerCase().includes(search) ||
        item.assetType.toLowerCase().includes(search) ||
        item.serialNumber.toLowerCase().includes(search) ||
        item.location.toLowerCase().includes(search);
      // statusFilter is irrelevant in decommissioned view
      const matchStatus =
        this.showingDecommissioned() || !status || item.conditionStatus === status;
      return matchSearch && matchStatus;
    });
  });

  // ── Pagination derived values ─────────────────────────────────────────────
  protected readonly page = computed<PaginatedList<SystemAssetListItem>>(() => {
    const pageNumber = this.currentPage();
    const size = this.pageSize();
    const filtered = this.filteredItems();
    const totalCount = filtered.length;
    const totalPages = Math.max(1, Math.ceil(totalCount / size));
    const start = (pageNumber - 1) * size;
    const items = filtered.slice(start, start + size);

    return {
      items,
      pageNumber,
      totalPages,
      totalCount,
      hasPreviousPage: pageNumber > 1,
      hasNextPage: pageNumber < totalPages,
    };
  });

  protected readonly isSubmittingForm = computed(
    () => this.createMutation.isPending() || this.updateMutation.isPending(),
  );

  constructor() {
    // Reset to page 1 whenever the filtered set changes (filter or view switch)
    effect(() => {
      this.filteredItems(); // track
      untracked(() => this.currentPage.set(1));
    });

    effect(() => {
      const detail = this.detailQuery.data();
      const currentId = this.editingId();
      if (detail && currentId === detail.id) {
        untracked(() => {
          this.formDialog().openEdit(detail);
          this.editingId.set(null);
        });
      }
    });

    effect(() => {
      const error = this.detailQuery.error();
      if (error) {
        untracked(() => {
          this.editingId.set(null);
        });
      }
    });
  }

  onFilterChanged(value: AssetFilterValue): void {
    this.search.set(value.search);
    this.statusFilter.set(value.conditionStatus);
  }

  onDecommissionedToggled(): void {
    this.showingDecommissioned.update((v) => !v);
    // Clear active filters that don't apply in decommissioned view
    if (this.showingDecommissioned()) {
      this.statusFilter.set('');
    }
  }

  onPageChanged(page: number): void {
    this.currentPage.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  onCreateClicked(): void {
    this.formDialog().openCreate();
  }

  onViewClicked(item: SystemAssetListItem): void {
    this.detailDialog().open(item);
  }

  onEditClicked(item: SystemAssetListItem): void {
    this.editingId.set(item.id);
  }

  onConditionChangeClicked(item: SystemAssetListItem): void {
    this.conditionDialog().open(item);
  }

  onDeleteClicked(item: SystemAssetListItem): void {
    // Hard delete: permanently removes the asset via DELETE endpoint
    this.deleteMutation.mutate(item.id, {
      onSuccess: () => toast.success(`"${item.assetName}" has been deleted.`),
    });
  }

  onFormSubmitted(event: AssetFormSubmit): void {
    if (event.mode === 'create') {
      const sn = event.payload.serialNumber.trim().toLowerCase();
      const isDuplicate = this.listQuery
        .data()
        ?.some((a) => a.serialNumber.toLowerCase() === sn);

      if (isDuplicate) {
        toast.error(`Serial Number "${event.payload.serialNumber}" already exists.`);
        return;
      }

      this.createMutation.mutate(event.payload, {
        onSuccess: () => {
          toast.success('Asset created successfully.');
          this.formDialog().close();
        },
      });
    } else {
      this.updateMutation.mutate(
        { id: event.id, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Asset updated successfully.');
            this.formDialog().close();
          },
        },
      );
    }
  }

  onConditionSubmitted(event: AssetConditionSubmit): void {
    this.updateConditionMutation.mutate(
      { id: event.id, payload: event.payload },
      {
        onSuccess: () => {
          toast.success('Condition updated.');
          this.conditionDialog().close();
        },
      },
    );
  }
}
