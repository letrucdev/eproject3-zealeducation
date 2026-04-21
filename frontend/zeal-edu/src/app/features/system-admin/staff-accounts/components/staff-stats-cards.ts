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
  template: `
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <section hlmCard>
        <div hlmCardHeader>
          <p hlmCardDescription class="flex items-center gap-2">
            <ng-icon hlm name="lucideUsers" size="sm" />
            Total Staff
          </p>
          @if (isLoading()) {
            <hlm-skeleton class="mt-2 h-7 w-20" />
          } @else {
            <h3 hlmCardTitle class="text-3xl">{{ stats()?.total ?? 0 }}</h3>
          }
        </div>
      </section>

      <section hlmCard>
        <div hlmCardHeader>
          <p hlmCardDescription class="flex items-center gap-2">
            <ng-icon hlm name="lucideCheckCheck" size="sm" />
            Active
          </p>
          @if (isLoading()) {
            <hlm-skeleton class="mt-2 h-7 w-20" />
          } @else {
            <h3 hlmCardTitle class="text-3xl">
              {{ stats()?.active ?? 0 }}
            </h3>
          }
        </div>
      </section>

      <section hlmCard>
        <div hlmCardHeader>
          <p hlmCardDescription class="flex items-center gap-2">
            <ng-icon hlm name="lucideCircleSlash" size="sm" class="text-muted-foreground" />
            Inactive
          </p>
          @if (isLoading()) {
            <hlm-skeleton class="mt-2 h-7 w-20" />
          } @else {
            <h3 hlmCardTitle class="text-muted-foreground text-3xl">
              {{ stats()?.inactive ?? 0 }}
            </h3>
          }
        </div>
      </section>

      <!--  <section hlmCard>
        <div hlmCardHeader>
          <p hlmCardDescription class="flex items-center gap-2">
            <ng-icon hlm name="lucideBriefcase" size="sm" />
            Breakdown by Role
          </p>
          @if (isLoading()) {
            <hlm-skeleton class="mt-2 h-7 w-32" />
          } @else {
            <div class="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-sm">
              <span>
                <span class="text-muted-foreground">Incharge:</span>
                <span class="ms-1 font-semibold">{{ stats()?.incharge ?? 0 }}</span>
              </span>
              <span>
                <span class="text-muted-foreground">Counselor:</span>
                <span class="ms-1 font-semibold">{{ stats()?.counselor ?? 0 }}</span>
              </span>
              <span>
                <span class="text-muted-foreground">Accounts:</span>
                <span class="ms-1 font-semibold">{{ stats()?.accountsStaff ?? 0 }}</span>
              </span>
            </div>
          }
        </div>
      </section> -->
    </div>
  `,
})
export class StaffStatsCards {
  readonly stats = input<StaffStatistics | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
}
