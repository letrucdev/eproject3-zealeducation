import { ChangeDetectionStrategy, Component, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { SystemAssetListItem } from '../../../../core/models/system-asset-list-item';
import { AssetConditionStatus } from '../../../../core/models/asset-condition-status';

@Component({
  selector: 'app-asset-detail-dialog',
  standalone: true,
  imports: [HlmDialogImports, HlmButtonImports, HlmBadgeImports, DatePipe],
  templateUrl: './asset-detail-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AssetDetailDialog {
  protected readonly dlg = viewChild.required<HlmDialog>('dlg');
  protected readonly asset = signal<SystemAssetListItem | null>(null);

  open(item: SystemAssetListItem): void {
    this.asset.set(item);
    this.dlg().open();
  }

  protected conditionVariant(s: AssetConditionStatus | undefined): 'default' | 'secondary' | 'destructive' | 'outline' {
    if (!s) return 'default';
    switch (s) {
      case AssetConditionStatus.Good: return 'default';
      case AssetConditionStatus.Maintenance: return 'secondary';
      case AssetConditionStatus.Faulty: return 'destructive';
      case AssetConditionStatus.Decommissioned: return 'outline';
      default: return 'default';
    }
  }
}
