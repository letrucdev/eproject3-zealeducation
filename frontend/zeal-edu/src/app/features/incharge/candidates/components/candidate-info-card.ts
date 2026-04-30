import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil, lucideScale } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { CandidateStatus } from '@core/models/candidate-status';
import { CandidateDetail } from '@core/models/candidate-detail';
import { CANDIDATE_STATUS_LABELS } from '../models/candidate-labels';

@Component({
  selector: 'app-candidate-info-card',
  imports: [
    DatePipe,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucidePencil, lucideScale })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'candidate-info-card.html',
})
export class CandidateInfoCard {
  readonly detail = input.required<CandidateDetail>();
  readonly editClicked = output<void>();
  readonly fineClicked = output<void>();

  protected readonly statuses = CandidateStatus;
  protected readonly statusLabels = CANDIDATE_STATUS_LABELS;
}
