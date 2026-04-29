import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { CandidatePortalService } from '../candidate-portal.service';
import { CandidateProfileEnrollmentsCard } from './components/profile-enrollments-card';
import { CandidateProfileInfoCard } from './components/profile-info-card';

@Component({
  selector: 'app-candidate-profile-page',
  imports: [
    HlmSkeletonImports,
    CandidateProfileInfoCard,
    CandidateProfileEnrollmentsCard,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2">
        <h1 class="text-2xl font-semibold tracking-tight">Profile</h1>
        <p class="text-muted-foreground text-sm">View your personal information and manage your password.</p>
      </header>

      @if (profileQuery.isPending()) {
        <div class="grid gap-4">
          <hlm-skeleton class="h-40" />
          <hlm-skeleton class="h-32" />
        </div>
      } @else if (profileQuery.isError()) {
        <div class="rounded-md border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">
          Failed to load profile.
        </div>
      } @else if (profileQuery.data(); as profile) {
        <app-candidate-profile-info-card [profile]="profile" />
        <app-candidate-profile-enrollments-card [profile]="profile" />
      }
    </section>
  `,
})
export default class CandidateProfilePage {
  private readonly _service = inject(CandidatePortalService);

  protected readonly profileQuery = this._service.profileQuery();
}
