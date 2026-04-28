import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { HttpStatusCode } from '@angular/common/http';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { toast } from '@spartan-ng/brain/sonner';
import { AuthService } from '@core/auth/auth-service';
import { AuthToken } from '@core/auth/auth-token';
import { resolveMessage } from '@core/http/error-interceptor';

const matchPasswords: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const newPassword = group.get('newPassword')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  if (!newPassword || !confirmPassword) return null;
  return newPassword === confirmPassword ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-change-password-form',
  imports: [
    ReactiveFormsModule,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmCardImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-6">
      <div hlmCard>
        <div hlmCardContent class="p-6 md:p-8">
          <form [formGroup]="form" (ngSubmit)="submit()">
            <hlm-field-group>
              <div class="flex flex-col items-center gap-2 text-center">
                <h1 class="text-2xl font-bold">Change your password</h1>
                <p class="text-muted-foreground text-balance text-sm">
                  For your security, you must set a new password before continuing.
                </p>
                @if (remainingMs() > 0) {
                  <p class="text-muted-foreground text-xs">
                    Session expires in
                    <span class="font-mono font-medium">{{ remainingLabel() }}</span>
                  </p>
                }
              </div>
              <hlm-field>
                <label hlmFieldLabel for="currentPassword">Current password</label>
                <input
                  hlmInput
                  type="password"
                  id="currentPassword"
                  formControlName="currentPassword"
                  autocomplete="current-password"
                />
                <hlm-field-error validator="required"
                  >Current password is required.</hlm-field-error
                >
                <hlm-field-error validator="incorrectPassword">
                  {{ form.controls.currentPassword.getError('incorrectPassword') }}
                </hlm-field-error>
              </hlm-field>
              <hlm-field>
                <label hlmFieldLabel for="newPassword">New password</label>
                <input
                  hlmInput
                  type="password"
                  id="newPassword"
                  formControlName="newPassword"
                  autocomplete="new-password"
                />
                <hlm-field-error validator="required">New password is required.</hlm-field-error>
                <hlm-field-error validator="minlength">
                  New password must be at least 8 characters long.
                </hlm-field-error>
              </hlm-field>
              <hlm-field>
                <label hlmFieldLabel for="confirmPassword">Confirm new password</label>
                <input
                  hlmInput
                  type="password"
                  id="confirmPassword"
                  formControlName="confirmPassword"
                  autocomplete="new-password"
                />
                <hlm-field-error validator="required"
                  >Please confirm your new password.</hlm-field-error
                >
                @if (form.hasError('passwordMismatch') && form.controls.confirmPassword.touched) {
                  <p class="text-destructive text-sm">Passwords do not match.</p>
                }
              </hlm-field>
              <hlm-field>
                <button
                  hlmBtn
                  type="submit"
                  [disabled]="form.invalid || auth.changePasswordMutation.isPending()"
                >
                  @if (auth.changePasswordMutation.isPending()) {
                    <hlm-spinner class="mr-2" />
                    Updating...
                  } @else {
                    Update password
                  }
                </button>
              </hlm-field>
            </hlm-field-group>
          </form>
        </div>
      </div>
    </div>
  `,
})
export class ChangePasswordForm {
  private readonly _fb = inject(FormBuilder);
  private readonly _authToken = inject(AuthToken);
  private readonly _destroyRef = inject(DestroyRef);
  protected readonly auth = inject(AuthService);

  protected readonly remainingMs = signal<number>(0);
  protected readonly remainingLabel = computed(() => {
    const totalSeconds = Math.max(0, Math.floor(this.remainingMs() / 1000));
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  });

  readonly form = this._fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: matchPasswords },
  );

  constructor() {
    effect(() => {
      const error = this.auth.changePasswordMutation.error();
      if (!error) return;
      if (error.status === HttpStatusCode.Unauthorized) {
        this.form.controls.currentPassword.setErrors({
          incorrectPassword: resolveMessage(error) || 'Current password is incorrect.',
        });
      }
    });

    this.form.controls.currentPassword.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      if (this.form.controls.currentPassword.hasError('incorrectPassword')) {
        this.form.controls.currentPassword.updateValueAndValidity();
      }
    });

    this._startSessionCountdown();
  }

  private _startSessionCountdown(): void {
    const expiresAt = this._authToken.expiresAt();
    if (expiresAt === null) return;

    const expiresAtMs = expiresAt.getTime();
    const tick = () => {
      const remaining = expiresAtMs - Date.now();
      this.remainingMs.set(Math.max(0, remaining));
      if (remaining <= 0) {
        clearInterval(intervalId);
        if (!this.auth.changePasswordMutation.isSuccess()) {
          toast.error('Your session has expired. Please sign in again.');
        }
        this.auth.signOut();
      }
    };

    tick();
    const intervalId = setInterval(tick, 1000);
    this._destroyRef.onDestroy(() => clearInterval(intervalId));
  }

  submit() {
    if (this.form.invalid) return;
    if (this.auth.changePasswordMutation.isPending()) return;
    const value = this.form.getRawValue();
    this.auth.changePasswordMutation.mutate({
      currentPassword: value.currentPassword,
      newPassword: value.newPassword,
    });
  }
}
