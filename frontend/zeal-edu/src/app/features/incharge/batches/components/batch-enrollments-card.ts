import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideEye, lucideUserPlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { BatchEnrollmentItem } from '@core/models/batch-enrollment';
import { EnrollmentStatus } from '@features/incharge/candidates/models/candidate-detail';
import { PaginatedList } from '@core/models/paginated-list';

@Component({
  selector: 'app-batch-enrollments-card',
  imports: [
    DatePipe,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
  ],
  providers: [provideIcons({ lucideEye, lucideUserPlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-enrollments-card.html',
})
export class BatchEnrollmentsCard {
  readonly page = input<PaginatedList<BatchEnrollmentItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly canAddCandidate = input<boolean>(true);

  readonly addClicked = output<void>();
  readonly viewClicked = output<BatchEnrollmentItem>();

  protected readonly enrollmentStatuses = EnrollmentStatus;
}
