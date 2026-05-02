import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  viewChild,
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
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { AuthService } from '@core/auth/auth-service';
import { resolveMessage } from '@core/http/error-interceptor';

const matchPasswords: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const newPassword = group.get('newPassword')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  if (!newPassword || !confirmPassword) return null;
  return newPassword === confirmPassword ? null : { passwordMismatch: true };
};

@Component({
  selector: 'app-candidate-change-password-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmButtonImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-md w-md"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          <h2 hlmDialogTitle>Change Password</h2>
          <p class="text-muted-foreground text-sm">
            Update your password. You will be signed out and asked to sign in again.
          </p>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="mt-2 flex flex-col gap-4">
          <hlm-field-group>
            <hlm-field>
              <label hlmFieldLabel for="profile-current-password">Current password</label>
              <input
                hlmInput
                type="password"
                id="profile-current-password"
                formControlName="currentPassword"
                autocomplete="current-password"
              />
              <hlm-field-error validator="required">Current password is required.</hlm-field-error>
              <hlm-field-error validator="incorrectPassword">
                {{ form.controls.currentPassword.getError('incorrectPassword') }}
              </hlm-field-error>
            </hlm-field>

            <hlm-field>
              <label hlmFieldLabel for="profile-new-password">New password</label>
              <input
                hlmInput
                type="password"
                id="profile-new-password"
                formControlName="newPassword"
                autocomplete="new-password"
              />
              <hlm-field-error validator="required">New password is required.</hlm-field-error>
              <hlm-field-error validator="minlength">
                New password must be at least 8 characters long.
              </hlm-field-error>
            </hlm-field>

            <hlm-field>
              <label hlmFieldLabel for="profile-confirm-password">Confirm new password</label>
              <input
                hlmInput
                type="password"
                id="profile-confirm-password"
                formControlName="confirmPassword"
                autocomplete="new-password"
              />
              <hlm-field-error validator="required">
                Please confirm your new password.
              </hlm-field-error>
              @if (form.hasError('passwordMismatch') && form.controls.confirmPassword.touched) {
                <p class="text-destructive text-sm">Passwords do not match.</p>
              }
            </hlm-field>
          </hlm-field-group>

          <div hlmDialogFooter>
            <button hlmBtn variant="outline" type="button" hlmDialogClose>Cancel</button>
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
          </div>
        </form>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class CandidateChangePasswordDialog {
  private readonly _fb = inject(FormBuilder);
  protected readonly auth = inject(AuthService);

  protected readonly dlg = viewChild<HlmDialog>('dlg');

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
  }

  open(): void {
    this.form.reset({ currentPassword: '', newPassword: '', confirmPassword: '' });
    this.auth.changePasswordMutation.reset();
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected submit(): void {
    if (this.form.invalid) return;
    if (this.auth.changePasswordMutation.isPending()) return;
    const value = this.form.getRawValue();
    this.auth.changePasswordMutation.mutate({
      currentPassword: value.currentPassword,
      newPassword: value.newPassword,
    });
  }
}
