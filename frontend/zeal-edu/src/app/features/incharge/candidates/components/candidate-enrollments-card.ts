import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { CandidateEnrollmentItem, EnrollmentStatus } from '../models/candidate-detail';
import { ENROLLMENT_STATUS_LABELS } from '../models/candidate-labels';

@Component({
  selector: 'app-candidate-enrollments-card',
  imports: [DatePipe, HlmCardImports, HlmBadgeImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-enrollments-card.html',
})
export class CandidateEnrollmentsCard {
  readonly enrollments = input.required<CandidateEnrollmentItem[]>();

  protected readonly enrollmentStatuses = EnrollmentStatus;
  protected readonly statusLabels = ENROLLMENT_STATUS_LABELS;
}
