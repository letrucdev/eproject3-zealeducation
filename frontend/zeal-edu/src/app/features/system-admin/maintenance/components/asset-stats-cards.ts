import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { AssetStatistics } from '../../../../core/models/asset-statistics';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideServer, lucideCheckCheck, lucideWrench, lucideAlertTriangle, lucideCircleSlash } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';

@Component({
  selector: 'app-asset-stats-cards',
  standalone: true,
  imports: [HlmCardImports, HlmSkeletonImports, NgIcon, HlmIconImports],
  providers: [provideIcons({ lucideServer, lucideCheckCheck, lucideWrench, lucideAlertTriangle, lucideCircleSlash })],
  templateUrl: './asset-stats-cards.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AssetStatsCards {
  readonly stats = input<AssetStatistics | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly showingDecommissioned = input<boolean>(false);

  readonly decommissionedToggled = output<void>();
}
