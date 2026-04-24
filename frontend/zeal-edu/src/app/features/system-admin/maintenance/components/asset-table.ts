import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, Directive, input, output, computed } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil, lucideActivity, lucideTrash2, lucideEye } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { NgIcon } from '@ng-icons/core';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmTooltipImports } from '@spartan-ng/helm/tooltip';
import { PaginatedList } from '../../../../core/models/paginated-list';
import { SystemAssetListItem } from '../../../../core/models/system-asset-list-item';
import { AssetConditionStatus } from '../../../../core/models/asset-condition-status';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '../../../../shared/components/data-table';

@Directive({
  selector: '[assetCell]',
  standalone: true,
  providers: [{ provide: DataTableCellDef, useExisting: AssetCellDef }],
})
export class AssetCellDef extends DataTableCellDef<SystemAssetListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'assetCell' });

  static override ngTemplateContextGuard(
    _dir: AssetCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<SystemAssetListItem> {
    return true;
  }
}

@Component({
  selector: 'app-asset-table',
  standalone: true,
  imports: [DataTable, AssetCellDef, DatePipe, HlmBadgeImports, HlmButtonImports, NgIcon, HlmIconImports, HlmTooltipImports],
  providers: [provideIcons({ lucidePencil, lucideActivity, lucideTrash2, lucideEye })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'asset-table.html',
})
export default class AssetTable {
  readonly page = input<PaginatedList<SystemAssetListItem> | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly showingDecommissioned = input<boolean>(false);

  readonly editClicked = output<SystemAssetListItem>();
  readonly viewClicked = output<SystemAssetListItem>();
  readonly deleteClicked = output<SystemAssetListItem>();
  readonly conditionChangeClicked = output<SystemAssetListItem>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();

  protected readonly columns: DataTableColumn<SystemAssetListItem>[] = [
    { key: 'assetName', header: 'Asset Name', width: 'w-48' },
    { key: 'assetType', header: 'Type', width: 'w-32' },
    { key: 'serialNumber', header: 'Serial No.', width: 'w-36' },
    { key: 'location', header: 'Location', width: 'w-48' },
    { key: 'conditionStatus', header: 'Condition', width: 'w-36', align: 'center' },
    { key: 'purchaseDate', header: 'Purchase Date', width: 'w-36' },
    { key: 'lastMaintenance', header: 'Last Maintenance', width: 'w-40' },
    { key: 'managedByName', header: 'Managed By', width: 'w-40' },
    { key: 'actions', header: 'Actions', width: 'w-36', align: 'right' },
  ];

  protected readonly trackById = (row: SystemAssetListItem): string => row.id;

  protected conditionLabel(s: AssetConditionStatus): string {
    return s;
  }

  protected conditionVariant(s: AssetConditionStatus): 'default' | 'secondary' | 'destructive' | 'outline' {
    switch (s) {
      case AssetConditionStatus.Good: return 'default';
      case AssetConditionStatus.Maintenance: return 'secondary';
      case AssetConditionStatus.Faulty: return 'destructive';
      case AssetConditionStatus.Decommissioned: return 'outline';
      default: return 'default';
    }
  }

}
