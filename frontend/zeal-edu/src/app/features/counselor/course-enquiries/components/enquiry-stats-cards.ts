import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCalendarClock,
  lucideCheckCheck,
  lucideClock,
  lucideInbox,
  lucideUsers,
} from '@ng-icons/lucide';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { EnquiryStatistics } from '@core/models/enquiry-statistics';

@Component({
  selector: 'app-enquiry-stats-cards',
  imports: [HlmCardImports, HlmIconImports, HlmSkeletonImports],
  providers: [
    provideIcons({
      lucideUsers,
      lucideInbox,
      lucideClock,
      lucideCheckCheck,
      lucideCalendarClock,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-stats-cards.html',
})
export class EnquiryStatsCards {
  readonly stats = input<EnquiryStatistics | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
}
