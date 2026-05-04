import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCircleDollarSign,
  lucideHandCoins,
  lucideReceipt,
  lucideTrendingUp,
} from '@ng-icons/lucide';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { FinancialReport } from '@core/models/financial-report';

@Component({
  selector: 'app-financial-stats-cards',
  imports: [HlmCardImports, HlmIconImports, HlmSkeletonImports, VndPipe],
  providers: [
    provideIcons({
      lucideTrendingUp,
      lucideHandCoins,
      lucideCircleDollarSign,
      lucideReceipt,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'financial-stats-cards.html',
})
export class FinancialStatsCards {
  readonly stats = input<FinancialReport | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
}
