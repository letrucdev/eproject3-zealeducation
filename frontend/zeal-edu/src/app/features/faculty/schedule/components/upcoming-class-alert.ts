import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideAlarmClock } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { FacultyService } from '../../faculty.service';

@Component({
  selector: 'app-upcoming-class-alert',
  imports: [HlmIconImports],
  providers: [provideIcons({ lucideAlarmClock })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (upcoming().length > 0) {
      <div
        role="alert"
        class="rounded-md border border-amber-300/70 bg-amber-50 p-4 text-amber-900 dark:border-amber-400/30 dark:bg-amber-950/40 dark:text-amber-100"
      >
        <div class="flex items-start gap-3">
          <ng-icon hlm name="lucideAlarmClock" class="mt-0.5 size-5 shrink-0" />
          <div class="flex-1">
            <p class="text-sm font-semibold">
              {{ upcoming().length === 1 ? 'Upcoming class' : 'Upcoming classes' }} ({{
                upcoming().length
              }}
              {{ upcoming().length === 1 ? 'session' : 'sessions' }} in the next 3 hours)
            </p>
            <ul class="mt-2 space-y-1 text-sm">
              @for (item of upcoming(); track item.sessionId) {
                <li class="flex flex-wrap items-center gap-x-2">
                  <span class="font-medium">{{ formatTime(item.startTime) }}</span>
                  <span class="text-amber-800 dark:text-amber-200/80"
                    >· Batch {{ item.batchCode }}</span
                  >
                  @if (item.location) {
                    <span class="text-amber-700/80 dark:text-amber-300/70">
                      · {{ item.location }}
                    </span>
                  }
                </li>
              }
            </ul>
          </div>
        </div>
      </div>
    }
  `,
})
export class UpcomingClassAlert {
  private readonly _service = inject(FacultyService);

  protected readonly query = this._service.upcomingSessionsQuery();
  protected readonly upcoming = computed(() => this.query.data() ?? []);

  protected formatTime(time: string): string {
    return time.length >= 5 ? time.slice(0, 5) : time;
  }
}
