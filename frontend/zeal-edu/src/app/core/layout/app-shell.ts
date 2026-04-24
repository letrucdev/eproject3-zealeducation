import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { provideIcons } from '@ng-icons/core';
import {
  lucideBookOpen,
  lucideGraduationCap,
  lucidePanelLeft,
  lucideScrollText,
  lucideUserPlus,
  lucideUserSearch,
  lucideWrench,
} from '@ng-icons/lucide';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { HlmBreadcrumbImports } from '@spartan-ng/helm/breadcrumb';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { CurrentUser } from '@core/auth/current-user';
import { buildBreadcrumbs } from './breadcrumbs';
import { navMenusForRole, ROLE_LABELS } from './nav-items';
import { SidebarUserCard } from './sidebar-user-card';
import { environment } from '@/environments/environment';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    HlmSidebarImports,
    HlmSeparatorImports,
    HlmIconImports,
    HlmBreadcrumbImports,
    SidebarUserCard,
  ],
  providers: [
    provideIcons({
      lucideWrench,
      lucideUserPlus,
      lucidePanelLeft,
      lucideGraduationCap,
      lucideScrollText,
      lucideUserSearch,
      lucideBookOpen,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'app-shell.html',
})
export class AppShell {
  readonly currentUser = inject(CurrentUser);
  readonly APP_NAME = environment.appName;
  readonly ROLE_LABEL = !!this.currentUser.role() ? ROLE_LABELS[this.currentUser.role()!] : '';

  private readonly router = inject(Router);

  private readonly navigationEnd = toSignal(
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)),
    { initialValue: null },
  );

  protected readonly visibleNavMenu = computed(() => navMenusForRole(this.currentUser.role()));

  protected readonly breadcrumbs = computed(() => {
    this.navigationEnd();
    return buildBreadcrumbs(this.router.routerState.snapshot.root);
  });
}
