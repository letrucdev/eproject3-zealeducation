import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideBookOpen, lucideCircleCheck, lucideCircleOff } from '@ng-icons/lucide';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { CourseStatistics } from '@core/models/course-statistics';

@Component({
  selector: 'app-course-stats-cards',
  imports: [HlmCardImports, HlmIconImports, HlmSkeletonImports],
  providers: [provideIcons({ lucideBookOpen, lucideCircleCheck, lucideCircleOff })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'course-stats-cards.html',
})
export class CourseStatsCards {
  readonly stats = input<CourseStatistics | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
}
