import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime } from 'rxjs/operators';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCheckboxImports } from '@spartan-ng/helm/checkbox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { BatchesService } from '../batches.service';
import { AssignableCandidatesQuery } from '../models/batch-payload';

export interface AssignCandidatesSubmit {
  batchId: string;
  enrollmentIds: string[];
}

@Component({
  selector: 'app-assign-candidates-dialog',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmCheckboxImports,
    HlmButtonImports,
    HlmSkeletonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'assign-candidates-dialog.html',
})
export class AssignCandidatesDialog {
  private readonly _service = inject(BatchesService);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<AssignCandidatesSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  protected readonly batchId = signal<string | null>(null);
  protected readonly isOpen = signal(false);
  protected readonly searchControl = new FormControl<string>('', { nonNullable: true });
  private readonly _searchSignal = toSignal(
    this.searchControl.valueChanges.pipe(takeUntilDestroyed(), debounceTime(250)),
    { initialValue: '' },
  );
  protected readonly page = signal(1);
  protected readonly pageSize = 10;
  protected readonly selectedIds = signal<Set<string>>(new Set());

  private readonly _params = computed<AssignableCandidatesQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize,
    search: this._searchSignal()?.trim() || undefined,
  }));

  protected readonly listQuery = this._service.assignableCandidatesQuery(
    this.batchId,
    this._params,
    this.isOpen,
  );

  protected readonly hasSelection = computed(() => this.selectedIds().size > 0);

  open(batchId: string): void {
    this.batchId.set(batchId);
    this.searchControl.setValue('', { emitEvent: true });
    this.page.set(1);
    this.selectedIds.set(new Set());
    this.isOpen.set(true);
    this.dlg()?.open();
  }

  close(): void {
    this.isOpen.set(false);
    this.dlg()?.close();
  }

  protected onToggle(enrollmentId: string): void {
    const next = new Set(this.selectedIds());
    if (next.has(enrollmentId)) {
      next.delete(enrollmentId);
    } else {
      next.add(enrollmentId);
    }
    this.selectedIds.set(next);
  }

  protected isSelected(enrollmentId: string): boolean {
    return this.selectedIds().has(enrollmentId);
  }

  protected onPrev(): void {
    if (this.page() > 1) this.page.update((p) => p - 1);
  }

  protected onNext(): void {
    const data = this.listQuery.data();
    if (data?.hasNextPage) this.page.update((p) => p + 1);
  }

  protected submit(): void {
    const id = this.batchId();
    if (!id) return;
    const ids = Array.from(this.selectedIds());
    if (ids.length === 0) return;
    this.submitted.emit({ batchId: id, enrollmentIds: ids });
  }
}
