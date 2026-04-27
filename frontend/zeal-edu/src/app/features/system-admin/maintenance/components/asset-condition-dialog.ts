import { ChangeDetectionStrategy, Component, inject, input, output, signal, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { SystemAssetListItem } from '../../../../core/models/system-asset-list-item';
import { UpdateAssetConditionPayload } from '../models/asset-payload';
import { AssetConditionStatus } from '../../../../core/models/asset-condition-status';

export interface AssetConditionSubmit {
  id: string;
  payload: UpdateAssetConditionPayload;
}

@Component({
  selector: 'app-asset-condition-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, HlmDialogImports, HlmFieldImports, HlmSelectImports, HlmButtonImports],
  templateUrl: './asset-condition-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AssetConditionDialog {
  private readonly _fb = inject(FormBuilder);
  readonly submitting = input<boolean>(false);
  readonly submitted = output<AssetConditionSubmit>();

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');

  protected readonly assetId = signal<string | null>(null);

  protected readonly form = this._fb.nonNullable.group({
    conditionStatus: [AssetConditionStatus.Good, Validators.required],
  });

  protected readonly statusLabel = (item: AssetConditionStatus) => item;

  open(item: SystemAssetListItem): void {
    this.assetId.set(item.id);
    this.form.setValue({
      conditionStatus: item.conditionStatus,
    });
    this.dlg().open();
  }

  close(): void {
    this.dlg().close();
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;
    const val = this.form.getRawValue();
    this.submitted.emit({
      id: this.assetId()!,
      payload: { conditionStatus: val.conditionStatus },
    });
  }
}
