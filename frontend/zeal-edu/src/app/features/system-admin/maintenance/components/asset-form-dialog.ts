import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, output, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { SystemAssetDetail } from '../../../../core/models/system-asset-detail';
import { CreateAssetPayload, UpdateAssetPayload } from '../models/asset-payload';

export interface AssetFormSubmitCreate { mode: 'create'; payload: CreateAssetPayload; }
export interface AssetFormSubmitEdit { mode: 'edit'; id: string; payload: UpdateAssetPayload; }
export type AssetFormSubmit = AssetFormSubmitCreate | AssetFormSubmitEdit;

@Component({
  selector: 'app-asset-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, HlmDialogImports, HlmFieldImports, HlmInputImports, HlmButtonImports, HlmSpinnerImports],
  templateUrl: './asset-form-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AssetFormDialog {
  private readonly _fb = inject(FormBuilder);
  readonly submitting = input<boolean>(false);
  readonly submitted = output<AssetFormSubmit>();

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');

  protected readonly mode = signal<'create' | 'edit'>('create');
  protected readonly assetId = signal<string | null>(null);
  protected readonly assetNameLabel = signal<string>('');

  protected readonly form = this._fb.nonNullable.group({
    assetName: ['', [Validators.required, Validators.maxLength(200)]],
    assetType: ['', Validators.required],
    serialNumber: ['', Validators.required],
    location: ['', Validators.required],
    purchaseDate: ['', Validators.required],
    notes: [''],
  });

  openCreate(): void {
    this.mode.set('create');
    this.assetId.set(null);
    this.assetNameLabel.set('');
    this.form.controls.serialNumber.enable();
    this.form.controls.purchaseDate.enable();
    this.form.reset();
    this.dlg().open();
  }

  openEdit(detail: SystemAssetDetail): void {
    this.mode.set('edit');
    this.assetId.set(detail.id);
    this.assetNameLabel.set(detail.assetName);
    this.form.setValue({
      assetName: detail.assetName,
      assetType: detail.assetType,
      serialNumber: detail.serialNumber,
      location: detail.location,
      purchaseDate: detail.purchaseDate.substring(0, 10),
      notes: detail.notes || '',
    });
    this.form.controls.serialNumber.disable();
    this.form.controls.purchaseDate.disable();
    this.dlg().open();
  }

  close(): void {
    this.dlg().close();
  }

  protected onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const val = this.form.getRawValue();
    if (this.mode() === 'create') {
      const payload: CreateAssetPayload = {
        ...val,
        purchaseDate: new Date(val.purchaseDate).toISOString()
      };
      this.submitted.emit({ mode: 'create', payload });
    } else {
      const payload: UpdateAssetPayload = {
        assetName: val.assetName,
        assetType: val.assetType,
        location: val.location,
        notes: val.notes
      };
      this.submitted.emit({ mode: 'edit', id: this.assetId()!, payload });
    }
  }
}
