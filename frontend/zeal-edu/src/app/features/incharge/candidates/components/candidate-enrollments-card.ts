import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { provideIcons } from '@ng-icons/core';
import { lucidePlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { CandidateEnrollmentItem, EnrollmentStatus } from '@core/models/candidate-detail';
import { ENROLLMENT_STATUS_LABELS } from '../models/candidate-labels';

@Component({
  selector: 'app-candidate-enrollments-card',
  imports: [
    DatePipe,
    RouterLink,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucidePlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-enrollments-card.html',
})
export class CandidateEnrollmentsCard {
  readonly enrollments = input.required<CandidateEnrollmentItem[]>();
  readonly addClicked = output<void>();

  protected readonly enrollmentStatuses = EnrollmentStatus;
  protected readonly statusLabels = ENROLLMENT_STATUS_LABELS;
}
