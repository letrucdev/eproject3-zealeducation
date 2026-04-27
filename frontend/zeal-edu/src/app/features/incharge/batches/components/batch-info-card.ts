import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil, lucideTrash2, lucideUserPlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { BatchDetail } from '@core/models/batch-detail';
import { BatchStatus } from '@core/models/batch-status';

@Component({
  selector: 'app-batch-info-card',
  imports: [DatePipe, HlmCardImports, HlmBadgeImports, HlmButtonImports, HlmIconImports],
  providers: [provideIcons({ lucidePencil, lucideUserPlus, lucideTrash2 })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-info-card.html',
})
export class BatchInfoCard {
  readonly detail = input.required<BatchDetail>();
  readonly editClicked = output<void>();
  readonly assignFacultyClicked = output<void>();
  readonly deleteClicked = output<void>();

  protected readonly statuses = BatchStatus;
}
