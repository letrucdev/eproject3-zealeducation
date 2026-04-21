import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideLogOut } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { AuthService } from '../auth/auth-service';
import { CurrentUser } from '../auth/current-user';
import { getUserInitials } from '../utils/user-initials';
import { ROLE_LABELS } from './nav-items';

@Component({
  selector: 'app-sidebar-user-card',
  imports: [HlmButtonImports, HlmIconImports],
  providers: [provideIcons({ lucideLogOut })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (currentUser.user(); as u) {
      <div class="flex items-center gap-2 px-1 py-1">
        <div
          class="bg-sidebar-accent text-sidebar-accent-foreground flex size-8 shrink-0 items-center justify-center rounded-full text-xs font-semibold"
          aria-hidden="true"
        >
          {{ initials() }}
        </div>
        <div
          class="flex min-w-0 flex-1 flex-col group-data-[collapsible=icon]:hidden"
        >
          <span class="truncate text-sm font-medium">{{ u.fullName }}</span>
          <span class="text-muted-foreground truncate text-xs">{{ roleLabel() }}</span>
        </div>
        <button
          hlmBtn
          variant="ghost"
          size="icon-sm"
          type="button"
          aria-label="Sign out"
          class="ml-auto group-data-[collapsible=icon]:hidden"
          (click)="auth.signOut()"
        >
          <ng-icon hlm name="lucideLogOut" size="sm" />
        </button>
      </div>
    }
  `,
})
export class SidebarUserCard {
  protected readonly currentUser = inject(CurrentUser);
  protected readonly auth = inject(AuthService);

  protected readonly initials = computed(() =>
    getUserInitials(this.currentUser.user()?.fullName),
  );

  protected readonly roleLabel = computed(() => {
    const role = this.currentUser.user()?.role;
    return role !== undefined ? ROLE_LABELS[role] : '';
  });
}
