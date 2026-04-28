import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCircleCheck,
  lucideCircleOff,
  lucideGraduationCap,
  lucideUserRoundX,
  lucideFlag,
} from '@ng-icons/lucide';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { BatchStatistics } from '@core/models/batch-statistics';

@Component({
  selector: 'app-batch-stats-cards',
  imports: [HlmCardImports, HlmIconImports, HlmSkeletonImports],
  providers: [
    provideIcons({
      lucideGraduationCap,
      lucideCircleCheck,
      lucideCircleOff,
      lucideUserRoundX,
      lucideFlag,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-stats-cards.html',
})
export class BatchStatsCards {
  readonly stats = input<BatchStatistics | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
}
