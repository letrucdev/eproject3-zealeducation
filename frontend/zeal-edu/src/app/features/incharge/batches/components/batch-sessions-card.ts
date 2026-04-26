import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideClipboardCheck,
  lucideCirclePlus,
  lucidePencil,
  lucideTrash2,
} from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { ClassSession, ClassSessionStatus } from '@core/models/class-session';

@Component({
  selector: 'app-batch-sessions-card',
  imports: [
    DatePipe,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSkeletonImports,
  ],
  providers: [
    provideIcons({ lucideClipboardCheck, lucideCirclePlus, lucidePencil, lucideTrash2 }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-sessions-card.html',
})
export class BatchSessionsCard {
  readonly sessions = input<ClassSession[] | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly canAddSession = input<boolean>(true);

  readonly addClicked = output<void>();
  readonly editClicked = output<ClassSession>();
  readonly deleteClicked = output<ClassSession>();
  readonly attendanceClicked = output<ClassSession>();

  protected readonly sessionStatuses = ClassSessionStatus;

  protected readonly trimSeconds = (time: string): string => time.substring(0, 5);
}
