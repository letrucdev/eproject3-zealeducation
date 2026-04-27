import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideSchool2 } from '@ng-icons/lucide';
import { LoginForm } from './login-form';

@Component({
  selector: 'app-login',
  imports: [RouterLink, LoginForm, NgIcon],
  providers: [provideIcons({ lucideSchool2 })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      class="bg-muted flex min-h-svh flex-col items-center justify-center gap-6 p-6 md:p-10"
    >
      <div class="flex w-full max-w-sm flex-col gap-6 md:max-w-4xl">
        <a routerLink="." class="flex items-center gap-2 self-center font-medium">
          <div
            class="bg-primary text-primary-foreground flex size-6 items-center justify-center rounded-md"
          >
            <ng-icon name="lucideSchool2" class="text-base" />
          </div>
          Zeal Education
        </a>
        <app-login-form />
      </div>
    </div>
  `,
})
export default class Login {}
