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

export type CourseActiveFilter = 'all' | 'true' | 'false';

export interface CourseFilterValue {
  search: string;
  isActive: boolean | null;
}

interface CourseFilterForm {
  search: string;
  active: CourseActiveFilter;
}

@Component({
  selector: 'app-course-filter-bar',
  imports: [
    ReactiveFormsModule,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    HlmIconImports,
  ],
  providers: [provideIcons({ lucideSearch, lucidePlus })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'course-filter-bar.html',
})
export class CourseFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input<CourseFilterValue>({ search: '', isActive: null });
  readonly filterChanged = output<CourseFilterValue>();
  readonly createClicked = output<void>();

  protected readonly activeLabel = (value: CourseActiveFilter): string => {
    switch (value) {
      case 'true':
        return 'Active';
      case 'false':
        return 'Inactive';
      default:
        return 'All Status';
    }
  };

  readonly form = this._fb.nonNullable.group<CourseFilterForm>({
    search: '',
    active: 'all',
  });

  ngOnInit(): void {
    const v = this.initial();
    this.form.patchValue(
      {
        search: v.search,
        active: v.isActive === null ? 'all' : v.isActive ? 'true' : 'false',
      },
      { emitEvent: false },
    );

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.active.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    const { search, active } = this.form.getRawValue();
    const isActive = active === 'all' ? null : active === 'true';
    this.filterChanged.emit({ search, isActive });
  }
}
