import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideBriefcase,
  lucideCheckCheck,
  lucideCircleSlash,
  lucideUsers,
} from '@ng-icons/lucide';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { StaffStatistics } from '../../../../core/models/staff-statistics';

@Component({
  selector: 'app-staff-stats-cards',
  imports: [HlmCardImports, HlmIconImports, HlmSkeletonImports],
  providers: [
    provideIcons({
      lucideUsers,
      lucideCheckCheck,
      lucideCircleSlash,
      lucideBriefcase,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'staff-stats-cards.html',
})
export class StaffStatsCards {
  readonly stats = input<StaffStatistics | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
}
