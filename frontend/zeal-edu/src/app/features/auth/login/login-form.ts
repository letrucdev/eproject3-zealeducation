import { ChangeDetectionStrategy, Component, effect, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { AuthService } from '@core/auth/auth-service';
import { CurrentUser } from '@core/auth/current-user';
import { navMenusForRole } from '@core/layout/nav-items';
import { HttpStatusCode } from '@angular/common/http';
import { resolveMessage } from '@core/http/error-interceptor';

@Component({
  selector: 'app-login-form',
  imports: [
    ReactiveFormsModule,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmCardImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-6">
      <div hlmCard class="overflow-hidden py-0!">
        <div hlmCardContent class="grid px-0! md:grid-cols-2">
          <form [formGroup]="form" (ngSubmit)="login()" class="p-6 md:p-8">
            <hlm-field-group>
              <div class="flex flex-col items-center gap-2 text-center">
                <h1 class="text-2xl font-bold">Welcome back</h1>
                <p class="text-muted-foreground text-balance">
                  Login to your Zeal Education account
                </p>
              </div>
              <hlm-field>
                <label hlmFieldLabel for="username">Username</label>
                <input
                  hlmInput
                  type="text"
                  id="username"
                  placeholder="user001"
                  formControlName="username"
                  autocomplete="username"
                />
                <hlm-field-error validator="required">Username is required.</hlm-field-error>
              </hlm-field>
              <hlm-field>
                <div class="flex items-center">
                  <label hlmFieldLabel for="password">Password</label>
                </div>
                <input
                  hlmInput
                  type="password"
                  id="password"
                  formControlName="password"
                  autocomplete="current-password"
                />
                <hlm-field-error validator="required">Password is required.</hlm-field-error>
                <hlm-field-error validator="minlength"
                  >Password must be at least 8 characters long.</hlm-field-error
                >
                <hlm-field-error validator="incorrectPassword">{{
                  this.form.controls.password.getError('incorrectPassword')
                }}</hlm-field-error>
              </hlm-field>
              <hlm-field>
                <button
                  hlmBtn
                  type="submit"
                  [disabled]="form.invalid || auth.loginMutation.isPending()"
                >
                  @if (auth.loginMutation.isPending()) {
                    Logging in...
                  } @else {
                    Login
                  }
                </button>
              </hlm-field>
            </hlm-field-group>
          </form>
          <div class="bg-muted relative hidden md:block">
            <img
              src="/signinbg.jpg"
              alt="Login background image"
              class="absolute inset-0 size-full object-cover dark:brightness-[0.2] dark:grayscale"
            />
          </div>
        </div>
      </div>
      <div
        class="text-muted-foreground *:[a]:hover:text-primary px-6 text-center text-xs text-balance *:[a]:underline *:[a]:underline-offset-4"
      >
        By clicking continue, you agree to our <a href="!#">Terms of Service</a>
        and <a href="!#">Privacy Policy</a>.
      </div>
    </div>
  `,
})
export class LoginForm {
  private readonly _fb = inject(FormBuilder);
  private readonly _currentUser = inject(CurrentUser);
  private readonly _router = inject(Router);
  protected readonly auth = inject(AuthService);

  readonly form = this._fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor() {
    effect(() => {
      const error = this.auth.loginMutation.error();
      if (error?.status !== HttpStatusCode.Unauthorized) return;

      this.form.controls.password.setErrors({
        incorrectPassword: resolveMessage(error) || 'Invalid username or password.',
      });
      this.form.controls.password.markAsTouched();
    });

    effect(() => {
      if (!this.auth.loginMutation.isSuccess()) return;
      if (this._currentUser.mustChangePassword()) {
        void this._router.navigateByUrl('/change-password');
        return;
      }
      const role = this._currentUser.role();
      const first = role !== undefined ? navMenusForRole(role)[0].items[0] : undefined;
      void this._router.navigateByUrl(first?.route ?? '/app');
    });

    this.form.controls.password.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      if (this.form.controls.password.hasError('incorrectPassword')) {
        this.form.controls.password.updateValueAndValidity();
      }
    });
  }

  login() {
    if (this.form.invalid) return;
    this.auth.loginMutation.mutate(this.form.getRawValue());
  }
}
