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
import { Router } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { BatchListItem } from '@core/models/batch-list-item';
import { BatchStatus } from '@core/models/batch-status';
import { BatchesService } from './batches.service';
import {
  BatchAssignFacultyDialog,
  BatchAssignFacultySubmit,
} from './components/batch-assign-faculty-dialog';
import { BatchFilterBar, BatchFilterValue } from './components/batch-filter-bar';
import { BatchFormDialog, BatchFormSubmit } from './components/batch-form-dialog';
import { BatchStatsCards } from './components/batch-stats-cards';
import { BatchTable } from './components/batch-table';
import { BatchListQuery } from './models/batch-payload';
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-batches-management-page',
  imports: [
    BatchStatsCards,
    BatchFilterBar,
    BatchTable,
    BatchFormDialog,
    BatchAssignFacultyDialog,
    ConfirmDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batches-management-page.html',
})
export default class BatchesManagementPage {
  private readonly _service = inject(BatchesService);
  private readonly _router = inject(Router);

  protected readonly formDialog = viewChild.required<BatchFormDialog>('formDialog');
  protected readonly assignDialog = viewChild.required<BatchAssignFacultyDialog>('assignDialog');
  protected readonly confirmDialog = viewChild.required<ConfirmDialog>('confirmDialog');

  private readonly _pendingDelete = signal<BatchListItem | null>(null);

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<BatchStatus | null>(null);

  protected readonly initialFilter: BatchFilterValue = { search: '', status: null };

  protected readonly focusedBatchId = signal<string | null>(null);

  private readonly _listParams = computed<BatchListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    status: this.statusFilter(),
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly statsQuery = this._service.statisticsQuery();
  protected readonly detailQuery = this._service.detailQuery(this.focusedBatchId);
  protected readonly createMutation = this._service.createMutation();
  protected readonly updateMutation = this._service.updateMutation();
  protected readonly assignFacultyMutation = this._service.assignFacultyMutation();
  protected readonly deleteMutation = this._service.deleteMutation();

  protected readonly isSubmittingForm = computed(
    () => this.createMutation.isPending() || this.updateMutation.isPending(),
  );

  constructor() {
    effect(() => {
      const detail = this.detailQuery.data();
      const currentId = this.focusedBatchId();
      if (!detail || currentId !== detail.batchId) return;

      untracked(() => {
        this.formDialog().openEdit(detail);
        this.focusedBatchId.set(null);
      });
    });

    effect(() => {
      const error = this.detailQuery.error();
      if (!error) return;
      untracked(() => this.focusedBatchId.set(null));
    });
  }

  onFilterChanged(value: BatchFilterValue): void {
    this.search.set(value.search);
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

  onViewClicked(row: BatchListItem): void {
    void this._router.navigate(['/app/incharge/batches', row.batchId]);
  }

  onEditClicked(row: BatchListItem): void {
    this.focusedBatchId.set(row.batchId);
  }

  onAssignFacultyClicked(row: BatchListItem): void {
    this.assignDialog().open(row);
  }

  onFormSubmitted(event: BatchFormSubmit): void {
    if (event.mode === 'create') {
      this.createMutation.mutate(event.payload, {
        onSuccess: () => {
          toast.success('Batch created successfully.');
          this.formDialog().close();
        },
      });
    } else {
      this.updateMutation.mutate(
        { batchId: event.batchId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Batch updated successfully.');
            this.formDialog().close();
          },
        },
      );
    }
  }

  onAssignFacultySubmitted(event: BatchAssignFacultySubmit): void {
    this.assignFacultyMutation.mutate(event, {
      onSuccess: () => {
        toast.success('Faculty assignment updated.');
        this.assignDialog().close();
      },
    });
  }

  onDeleteClicked(row: BatchListItem): void {
    this._pendingDelete.set(row);
    this.confirmDialog().open({
      title: 'Delete batch',
      message: `This will permanently delete batch ${row.batchCode}. This action cannot be undone.`,
      confirmLabel: 'Delete',
      destructive: true,
    });
  }

  onDeleteConfirmed(): void {
    const row = this._pendingDelete();
    if (!row) return;
    this._pendingDelete.set(null);
    this.deleteMutation.mutate(row.batchId, {
      onSuccess: () => toast.success('Batch deleted successfully.'),
    });
  }

  onDeleteCancelled(): void {
    this._pendingDelete.set(null);
  }
}
