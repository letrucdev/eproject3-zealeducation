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
  templateUrl: 'staff-filter-bar.html',
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
