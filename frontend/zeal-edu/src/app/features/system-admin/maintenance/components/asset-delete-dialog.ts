import { ChangeDetectionStrategy, Component, output, signal, viewChild } from '@angular/core';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { SystemAssetListItem } from '../../../../core/models/system-asset-list-item';

@Component({
  selector: 'app-asset-delete-dialog',
  standalone: true,
  imports: [HlmDialogImports, HlmButtonImports],
  templateUrl: './asset-delete-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AssetDeleteDialog {
  readonly confirmed = output<SystemAssetListItem>();

  protected readonly dlg = viewChild.required<HlmDialog>('dlg');
  protected readonly asset = signal<SystemAssetListItem | null>(null);

  open(asset: SystemAssetListItem): void {
    this.asset.set(asset);
    this.dlg().open();
  }

  close(): void {
    this.dlg().close();
  }

  protected onConfirm(): void {
    const a = this.asset();
    if (a) {
      this.confirmed.emit(a);
      this.close();
    }
  }
}
