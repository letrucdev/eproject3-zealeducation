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
    <div class="grid min-h-svh lg:grid-cols-2">
      <div class="flex flex-col gap-4 p-6 md:p-10">
        <div class="flex justify-center gap-2 md:justify-start">
          <a routerLink="." class="flex items-center gap-2 font-medium">
            <div
              class="bg-primary text-primary-foreground flex size-6 items-center justify-center rounded-md"
            >
              <ng-icon name="lucideSchool2" class="text-base" />
            </div>
            Zeal Education
          </a>
        </div>
        <div class="flex flex-1 items-center justify-center">
          <div class="w-full max-w-md">
            <app-login-form />
          </div>
        </div>
      </div>
      <div class="relative hidden lg:block">
        <img
          src="/signinbg.jpg"
          alt="Login background image"
          class="size-full object-cover"
        />
      </div>
    </div>
  `,
})
export default class Login {}
