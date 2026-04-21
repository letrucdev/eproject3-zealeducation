import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { Gender } from '../../../../core/models/gender';
import { StaffDetail } from '../../../../core/models/staff-detail';
import { UserRole } from '../../../../core/models/user-role';
import { CreateStaffPayload, UpdateStaffPayload } from '../models/staff-form-payload';
import { ROLE_LABELS } from '../../../../core/layout/nav-items';

export type StaffFormMode = 'create' | 'edit';

export interface StaffFormSubmitCreate {
  mode: 'create';
  payload: CreateStaffPayload;
}
export interface StaffFormSubmitUpdate {
  mode: 'edit';
  staffId: string;
  payload: UpdateStaffPayload;
}
export type StaffFormSubmit = StaffFormSubmitCreate | StaffFormSubmitUpdate;

@Component({
  selector: 'app-staff-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-4xl w-3xl flex max-h-[90dvh] flex-col"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          <h2 hlmDialogTitle>
            {{ isEdit() ? 'Edit Staff Account' : 'Add Staff Account' }}
          </h2>
          <p class="text-muted-foreground text-sm">
            {{
              isEdit()
                ? 'Update staff details. Username cannot be changed.'
                : 'Create a new staff account with a temporary password.'
            }}
          </p>
        </div>
        <form
          [formGroup]="form"
          (ngSubmit)="submit()"
          class="mt-2 flex min-h-0 flex-1 flex-col gap-4"
        >
          <hlm-field-group class="min-h-0 flex-1 overflow-y-auto">
            <div class="grid gap-4 sm:grid-cols-2">
              <hlm-field>
                <label hlmFieldLabel for="staff-username">Username</label>
                <input
                  hlmInput
                  id="staff-username"
                  type="text"
                  formControlName="username"
                  autocomplete="off"
                  class="w-full"
                />
                <hlm-field-error validator="required">Username is required.</hlm-field-error>
                <hlm-field-error validator="minlength">
                  Username must be at least 3 characters.
                </hlm-field-error>
                <hlm-field-error validator="pattern">
                  Only letters, digits, dot and underscore are allowed.
                </hlm-field-error>
              </hlm-field>

              <hlm-field>
                <label hlmFieldLabel for="staff-fullname">Full Name</label>
                <input
                  hlmInput
                  id="staff-fullname"
                  type="text"
                  formControlName="fullName"
                  class="w-full"
                />
                <hlm-field-error validator="required">Full name is required.</hlm-field-error>
              </hlm-field>
            </div>

            @if (!isEdit()) {
              <hlm-field>
                <label hlmFieldLabel for="staff-password">Temporary password</label>
                <input
                  hlmInput
                  id="staff-password"
                  type="password"
                  formControlName="password"
                  autocomplete="new-password"
                  class="w-full"
                />
                <hlm-field-error validator="required">Password is required.</hlm-field-error>
                <hlm-field-error validator="minlength"> Minimum 8 characters. </hlm-field-error>
              </hlm-field>
            }

            <hlm-field>
              <label hlmFieldLabel for="staff-email">Email</label>
              <input
                hlmInput
                id="staff-email"
                type="email"
                formControlName="email"
                autocomplete="email"
                class="w-full"
              />
              <hlm-field-error validator="required">Email is required.</hlm-field-error>
              <hlm-field-error validator="email">Invalid email format.</hlm-field-error>
            </hlm-field>

            <hlm-field>
              <label hlmFieldLabel for="staff-phone">Phone</label>
              <input
                hlmInput
                id="staff-phone"
                type="tel"
                formControlName="phone"
                autocomplete="tel"
                class="w-full"
              />
              <hlm-field-error validator="required">Phone is required.</hlm-field-error>
              <hlm-field-error validator="minLength"
                >Phone number must be 10 numbers</hlm-field-error
              >
              <hlm-field-error validator="maxLength"
                >Phone number must be 10 numbers</hlm-field-error
              >
            </hlm-field>

            <div class="grid gap-4 sm:grid-cols-2">
              <hlm-field>
                <label hlmFieldLabel for="staff-dob">Date of birth</label>
                <input hlmInput id="staff-dob" type="date" formControlName="dob" class="w-full" />
                <hlm-field-error validator="required">Date of birth is required.</hlm-field-error>
              </hlm-field>

              <hlm-field>
                <label hlmFieldLabel for="staff-gender">Gender</label>
                <hlm-select formControlName="gender" [itemToString]="genderLabel" class="w-full">
                  <hlm-select-trigger class="w-full">
                    <hlm-select-value placeholder="Select gender" />
                  </hlm-select-trigger>
                  <hlm-select-content *hlmSelectPortal>
                    <hlm-select-item [value]="genders.Male">Male</hlm-select-item>
                    <hlm-select-item [value]="genders.Female">Female</hlm-select-item>
                    <hlm-select-item [value]="genders.Other">Other</hlm-select-item>
                  </hlm-select-content>
                </hlm-select>
              </hlm-field>
            </div>

            <div class="grid gap-4 sm:grid-cols-3">
              <hlm-field>
                <label hlmFieldLabel for="staff-role">Role</label>
                <hlm-select formControlName="role" [itemToString]="roleLabel" class="w-full">
                  <hlm-select-trigger class="w-full">
                    <hlm-select-value placeholder="Select role" />
                  </hlm-select-trigger>
                  <hlm-select-content *hlmSelectPortal>
                    <hlm-select-item [value]="roles.Incharge">Incharge</hlm-select-item>
                    <hlm-select-item [value]="roles.Counselor">Counselor</hlm-select-item>
                    <hlm-select-item [value]="roles.AccountsStaff">Accounts Staff</hlm-select-item>
                  </hlm-select-content>
                </hlm-select>
              </hlm-field>

              <hlm-field>
                <label hlmFieldLabel for="staff-position">Position</label>
                <input
                  hlmInput
                  id="staff-position"
                  type="text"
                  formControlName="position"
                  class="w-full"
                />
                <hlm-field-error validator="required">Position is required.</hlm-field-error>
              </hlm-field>

              <hlm-field>
                <label hlmFieldLabel for="staff-department">Department</label>
                <input
                  hlmInput
                  id="staff-department"
                  type="text"
                  formControlName="department"
                  class="w-full"
                />
                <hlm-field-error validator="required">Department is required.</hlm-field-error>
              </hlm-field>
            </div>

            <hlm-field>
              <label hlmFieldLabel for="staff-joined">Joined date</label>
              <input
                hlmInput
                id="staff-joined"
                type="date"
                formControlName="joinedDate"
                class="w-full"
              />
            </hlm-field>

            @if (isEdit()) {
              <hlm-field>
                <label hlmFieldLabel for="staff-status">Status</label>
                <hlm-select formControlName="isActive" [itemToString]="statusLabel" class="w-full">
                  <hlm-select-trigger class="w-full">
                    <hlm-select-value placeholder="Select status" />
                  </hlm-select-trigger>
                  <hlm-select-content *hlmSelectPortal>
                    <hlm-select-item [value]="true">Active</hlm-select-item>
                    <hlm-select-item [value]="false">Inactive</hlm-select-item>
                  </hlm-select-content>
                </hlm-select>
              </hlm-field>
            }
          </hlm-field-group>

          <div hlmDialogFooter class="shrink-0">
            <button hlmBtn variant="outline" type="button" hlmDialogClose>Cancel</button>
            <button hlmBtn type="submit" [disabled]="form.invalid || submitting()">
              {{ submitting() ? 'Saving...' : isEdit() ? 'Save changes' : 'Create staff' }}
            </button>
          </div>
        </form>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class StaffFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<StaffFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly genders = Gender;
  protected readonly roles = UserRole;

  protected readonly genderLabel = (value: Gender): string => {
    switch (value) {
      case Gender.Male:
        return 'Male';
      case Gender.Female:
        return 'Female';
      case Gender.Other:
        return 'Other';
      default:
        return '';
    }
  };

  protected readonly roleLabel = (value: UserRole): string => ROLE_LABELS[value];

  protected readonly statusLabel = (value: boolean): string => (value ? 'Active' : 'Inactive');

  readonly mode = signal<StaffFormMode>('create');
  readonly initial = signal<StaffDetail | null>(null);

  readonly isEdit = computed(() => this.mode() === 'edit');

  readonly form = this._fb.nonNullable.group({
    username: [
      '',
      [Validators.required, Validators.minLength(3), Validators.pattern(/^[a-zA-Z0-9_.]+$/)],
    ],
    password: ['', [Validators.required, Validators.minLength(8)]],
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    phone: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(10)]],
    dob: ['', [Validators.required]],
    gender: [Gender.Male, [Validators.required]],
    role: [UserRole.Incharge, [Validators.required]],
    position: ['', [Validators.required, Validators.maxLength(60)]],
    department: ['', [Validators.required, Validators.maxLength(60)]],
    joinedDate: [this._today()],
    isActive: [true],
  });

  constructor() {
    effect(() => {
      if (this.isEdit()) {
        this.form.controls.username.disable({ emitEvent: false });
        this.form.controls.password.disable({ emitEvent: false });
        this.form.controls.joinedDate.disable({ emitEvent: false });
      } else {
        this.form.controls.username.enable({ emitEvent: false });
        this.form.controls.password.enable({ emitEvent: false });
        this.form.controls.joinedDate.enable({ emitEvent: false });
      }
    });
  }

  openCreate(): void {
    this.mode.set('create');
    this.initial.set(null);
    this.form.reset({
      username: '',
      password: '',
      fullName: '',
      email: '',
      phone: '',
      dob: '',
      gender: Gender.Male,
      role: UserRole.Incharge,
      position: '',
      department: '',
      joinedDate: this._today(),
      isActive: true,
    });
    this.dlg()?.open();
  }

  openEdit(detail: StaffDetail): void {
    this.mode.set('edit');
    this.initial.set(detail);
    this.form.reset({
      username: detail.username,
      password: '',
      fullName: detail.fullName,
      email: detail.email,
      phone: detail.phone,
      dob: detail.dob,
      gender: detail.gender,
      role: detail.role,
      position: detail.position,
      department: detail.department,
      joinedDate: detail.joinedDate,
      isActive: detail.isActive,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();

    if (this.isEdit()) {
      const detail = this.initial();
      if (!detail) return;
      this.submitted.emit({
        mode: 'edit',
        staffId: detail.staffId,
        payload: {
          fullName: v.fullName,
          email: v.email,
          phone: v.phone,
          dob: v.dob,
          gender: v.gender,
          role: v.role,
          position: v.position,
          department: v.department,
          joinedDate: v.joinedDate,
          isActive: v.isActive,
        },
      });
    } else {
      this.submitted.emit({
        mode: 'create',
        payload: {
          username: v.username,
          password: v.password,
          fullName: v.fullName,
          email: v.email,
          phone: v.phone,
          dob: v.dob,
          gender: v.gender,
          role: v.role,
          position: v.position,
          department: v.department,
          joinedDate: v.joinedDate || null,
        },
      });
    }
  }

  private _today(): string {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(
      now.getDate(),
    ).padStart(2, '0')}`;
  }
}
