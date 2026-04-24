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
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { BatchListItem } from '@core/models/batch-list-item';
import { FacultyListItem } from '@core/models/faculty-list-item';
import { FacultiesService } from '@core/services/faculties.service';
import { AssignFacultyPayload } from '../models/batch-payload';

export interface BatchAssignFacultySubmit {
  batchId: string;
  payload: AssignFacultyPayload;
}

@Component({
  selector: 'app-batch-assign-faculty-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmComboboxImports,
    HlmSpinnerImports,
    HlmButtonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-assign-faculty-dialog.html',
})
export class BatchAssignFacultyDialog {
  private readonly _fb = inject(FormBuilder);
  private readonly _facultiesService = inject(FacultiesService);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<BatchAssignFacultySubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly batch = signal<BatchListItem | null>(null);

  readonly form = this._fb.group({
    faculty: this._fb.control<FacultyListItem | null>(null),
  });

  protected readonly facultySearch = signal('');

  protected readonly faculties = this._facultiesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 20,
      search: this.facultySearch(),
    })),
  );

  protected readonly facultyItemToString = (f: FacultyListItem | null): string =>
    f ? `${f.fullName} (${f.facultyCode})` : '';

  open(batch: BatchListItem): void {
    this.batch.set(batch);
    this.facultySearch.set('');
    this.form.reset({ faculty: null });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    const b = this.batch();
    if (!b) return;
    const v = this.form.getRawValue();
    this.submitted.emit({
      batchId: b.batchId,
      payload: { facultyId: v.faculty?.facultyId ?? null },
    });
  }
}
