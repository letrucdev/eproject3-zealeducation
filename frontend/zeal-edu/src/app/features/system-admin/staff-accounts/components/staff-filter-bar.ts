import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import { lucidePlus, lucideSearch } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { UserRole } from '../../../../core/models/user-role';
import { ROLE_LABELS } from '../../../../core/layout/nav-items';

export interface StaffFilterValue {
  search: string;
  role: UserRole | '';
  status: 'all' | 'active' | 'inactive';
}

@Component({
  selector: 'app-staff-filter-bar',
  imports: [
    ReactiveFormsModule,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideSearch, lucidePlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <form
      [formGroup]="form"
      class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between"
    >
      <div class="flex flex-col gap-3 md:flex-row md:items-center md:gap-2">
        <div class="relative w-full md:w-72">
          <ng-icon
            hlm
            name="lucideSearch"
            size="sm"
            class="text-muted-foreground pointer-events-none absolute inset-y-0 start-3 my-auto"
          />
          <input
            hlmInput
            type="search"
            class="w-full ps-9"
            placeholder="Search by name, email, or username..."
            formControlName="search"
            aria-label="Search staff"
          />
        </div>

        <hlm-select formControlName="role" [itemToString]="roleLabel" class="w-full md:w-44">
          <hlm-select-trigger>
            <hlm-select-value placeholder="Role: All" />
          </hlm-select-trigger>
          <hlm-select-content *hlmSelectPortal>
            <hlm-select-item value="">All Roles</hlm-select-item>
            <hlm-select-item [value]="roles.Incharge">Incharge</hlm-select-item>
            <hlm-select-item [value]="roles.Counselor">Counselor</hlm-select-item>
            <hlm-select-item [value]="roles.AccountsStaff">Accounts Staff</hlm-select-item>
          </hlm-select-content>
        </hlm-select>

        <hlm-select formControlName="status" [itemToString]="statusLabel" class="w-full md:w-44">
          <hlm-select-trigger>
            <hlm-select-value placeholder="Status: All" />
          </hlm-select-trigger>
          <hlm-select-content *hlmSelectPortal>
            <hlm-select-item value="all">All Status</hlm-select-item>
            <hlm-select-item value="active">Active</hlm-select-item>
            <hlm-select-item value="inactive">Inactive</hlm-select-item>
          </hlm-select-content>
        </hlm-select>
      </div>

      <button hlmBtn type="button" (click)="createClicked.emit()">
        <ng-icon hlm name="lucidePlus" size="sm" />
        Add User
      </button>
    </form>
  `,
})
export class StaffFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input<StaffFilterValue>({
    search: '',
    role: '',
    status: 'all',
  });

  readonly filterChanged = output<StaffFilterValue>();
  readonly createClicked = output<void>();

  protected readonly roles = UserRole;

  protected readonly roleLabel = (value: UserRole): string => ROLE_LABELS[value] ?? 'All Roles';

  protected readonly statusLabel = (value: 'all' | 'active' | 'inactive'): string => {
    switch (value) {
      case 'active':
        return 'Active';
      case 'inactive':
        return 'Inactive';
      default:
        return 'All Status';
    }
  };

  readonly form = this._fb.nonNullable.group({
    search: '',
    role: '' as UserRole | '',
    status: 'all' as 'all' | 'active' | 'inactive',
  });

  ngOnInit(): void {
    this.form.patchValue(this.initial(), { emitEvent: false });

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.role.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.status.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    this.filterChanged.emit(this.form.getRawValue());
  }
}
