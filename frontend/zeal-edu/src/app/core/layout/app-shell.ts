import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import {
  lucideGraduationCap,
  lucidePanelLeft,
  lucideUserPlus,
  lucideWrench,
} from '@ng-icons/lucide';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { CurrentUser } from '../auth/current-user';
import { navMenusForRole } from './nav-items';
import { SidebarUserCard } from './sidebar-user-card';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    HlmSidebarImports,
    HlmSeparatorImports,
    HlmIconImports,
    SidebarUserCard,
  ],
  providers: [
    provideIcons({
      lucideWrench,
      lucideUserPlus,
      lucidePanelLeft,
      lucideGraduationCap,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-sidebar-wrapper>
      <hlm-sidebar collapsible="icon" variant="inset">
        <hlm-sidebar-header>
          <div class="flex items-center gap-2 px-1 py-1">
            <div
              class="bg-sidebar-primary text-sidebar-primary-foreground flex size-8 shrink-0 items-center justify-center rounded-md"
              aria-hidden="true"
            >
              <ng-icon hlm name="lucideGraduationCap" size="sm" />
            </div>
            <div class="flex min-w-0 flex-col group-data-[collapsible=icon]:hidden">
              <span class="truncate text-sm font-semibold">Zeal Education</span>
              <span class="text-muted-foreground truncate text-xs">Admin Console</span>
            </div>
          </div>
        </hlm-sidebar-header>
        <hlm-sidebar-content>
          @for (menu of visibleNavMenu(); track menu.title) {
            <hlm-sidebar-group>
              <div hlmSidebarGroupLabel>{{ menu.title }}</div>
              <div hlmSidebarGroupContent>
                <ul hlmSidebarMenu>
                  @for (item of menu.items; track item.route) {
                    <li hlmSidebarMenuItem>
                      <a
                        hlmSidebarMenuButton
                        [routerLink]="item.route"
                        routerLinkActive
                        #rla="routerLinkActive"
                        [isActive]="rla.isActive"
                        [tooltip]="item.label"
                      >
                        <ng-icon hlm [name]="item.icon" size="sm" />
                        <span>{{ item.label }}</span>
                      </a>
                    </li>
                  } @empty {
                    <li class="text-muted-foreground px-2 py-1.5 text-xs">
                      No menu items available.
                    </li>
                  }
                </ul>
              </div>
            </hlm-sidebar-group>
          }
        </hlm-sidebar-content>
        <hlm-sidebar-footer>
          <app-sidebar-user-card />
        </hlm-sidebar-footer>
        <button hlmSidebarRail aria-label="Toggle sidebar"></button>
      </hlm-sidebar>
      <main hlmSidebarInset>
        <header class="flex h-14 items-center gap-2 border-b px-4">
          <button hlmSidebarTrigger type="button" aria-label="Toggle sidebar"></button>
          <hlm-separator orientation="vertical" class="mx-1 h-4" />
        </header>
        <div class="flex-1 p-4">
          <router-outlet />
        </div>
      </main>
    </hlm-sidebar-wrapper>
  `,
})
export class AppShell {
  private readonly currentUser = inject(CurrentUser);

  protected readonly visibleNavMenu = computed(() => navMenusForRole(this.currentUser.role()));
}
