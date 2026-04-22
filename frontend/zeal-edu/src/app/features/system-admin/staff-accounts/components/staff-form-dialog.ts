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
import { toSignal } from '@angular/core/rxjs-interop';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { Gender } from '../../../../core/models/gender';
import { StaffDetail } from '../../../../core/models/staff-detail';
import { UserRole } from '../../../../core/models/user-role';
import {
  CreateFacultyPayload,
  CreateStaffPayload,
  UpdateFacultyPayload,
  UpdateStaffPayload,
} from '../models/staff-form-payload';
import { ROLE_LABELS } from '../../../../core/layout/nav-items';

export type StaffFormMode = 'create' | 'edit';

export interface StaffFormSubmitCreateStaff {
  kind: 'staff';
  mode: 'create';
  payload: CreateStaffPayload;
}
export interface StaffFormSubmitUpdateStaff {
  kind: 'staff';
  mode: 'edit';
  staffId: string;
  payload: UpdateStaffPayload;
}
export interface StaffFormSubmitCreateFaculty {
  kind: 'faculty';
  mode: 'create';
  payload: CreateFacultyPayload;
}
export interface StaffFormSubmitUpdateFaculty {
  kind: 'faculty';
  mode: 'edit';
  staffId: string;
  payload: UpdateFacultyPayload;
}
export type StaffFormSubmit =
  | StaffFormSubmitCreateStaff
  | StaffFormSubmitUpdateStaff
  | StaffFormSubmitCreateFaculty
  | StaffFormSubmitUpdateFaculty;

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
  templateUrl: 'staff-form-dialog.html',
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
    facultyCode: [''],
    qualification: [''],
    specialization: [''],
    experienceYears: [0],
  });

  private readonly _roleValue = toSignal(this.form.controls.role.valueChanges, {
    initialValue: this.form.controls.role.value,
  });

  protected readonly isFacultyRole = computed(() => this._roleValue() === UserRole.Faculty);

  protected readonly canSelectFaculty = computed(() => {
    if (!this.isEdit()) return true;
    return this.initial()?.role === UserRole.Faculty;
  });

  protected readonly canSelectNonFaculty = computed(() => {
    if (!this.isEdit()) return true;
    return this.initial()?.role !== UserRole.Faculty;
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

    effect(() => {
      const facultyValidators = [Validators.required, Validators.maxLength(100)];
      const codeValidators = [Validators.required, Validators.maxLength(20)];
      const expValidators = [Validators.required, Validators.min(0), Validators.max(80)];
      if (this.isFacultyRole()) {
        this.form.controls.facultyCode.setValidators(codeValidators);
        this.form.controls.qualification.setValidators(facultyValidators);
        this.form.controls.specialization.setValidators(facultyValidators);
        this.form.controls.experienceYears.setValidators(expValidators);
      } else {
        this.form.controls.facultyCode.clearValidators();
        this.form.controls.qualification.clearValidators();
        this.form.controls.specialization.clearValidators();
        this.form.controls.experienceYears.clearValidators();
      }
      this.form.controls.facultyCode.updateValueAndValidity({ emitEvent: false });
      this.form.controls.qualification.updateValueAndValidity({ emitEvent: false });
      this.form.controls.specialization.updateValueAndValidity({ emitEvent: false });
      this.form.controls.experienceYears.updateValueAndValidity({ emitEvent: false });
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
      facultyCode: '',
      qualification: '',
      specialization: '',
      experienceYears: 0,
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
      facultyCode: detail.facultyCode ?? '',
      qualification: detail.qualification ?? '',
      specialization: detail.specialization ?? '',
      experienceYears: detail.experienceYears ?? 0,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    const isFaculty = v.role === UserRole.Faculty;

    if (this.isEdit()) {
      const detail = this.initial();
      if (!detail) return;
      if (isFaculty) {
        this.submitted.emit({
          kind: 'faculty',
          mode: 'edit',
          staffId: detail.staffId,
          payload: {
            fullName: v.fullName,
            email: v.email,
            phone: v.phone,
            dob: v.dob,
            gender: v.gender,
            position: v.position,
            department: v.department,
            joinedDate: v.joinedDate,
            isActive: v.isActive,
            facultyCode: v.facultyCode,
            qualification: v.qualification,
            specialization: v.specialization,
            experienceYears: Number(v.experienceYears),
          },
        });
      } else {
        this.submitted.emit({
          kind: 'staff',
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
      }
    } else {
      if (isFaculty) {
        this.submitted.emit({
          kind: 'faculty',
          mode: 'create',
          payload: {
            username: v.username,
            password: v.password,
            fullName: v.fullName,
            email: v.email,
            phone: v.phone,
            dob: v.dob,
            gender: v.gender,
            position: v.position,
            department: v.department,
            joinedDate: v.joinedDate || null,
            facultyCode: v.facultyCode,
            qualification: v.qualification,
            specialization: v.specialization,
            experienceYears: Number(v.experienceYears),
          },
        });
      } else {
        this.submitted.emit({
          kind: 'staff',
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
  }

  private _today(): string {
    const now = new Date();
    return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(
      now.getDate(),
    ).padStart(2, '0')}`;
  }
}
