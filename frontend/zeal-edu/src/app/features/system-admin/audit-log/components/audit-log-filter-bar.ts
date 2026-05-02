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
import { lucideSearch } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuditAction } from '@core/models/audit-action';

export interface AuditLogFilterValue {
  search: string;
  action: AuditAction | '';
  tableName: string;
  fromDate: string;
  toDate: string;
}

@Component({
  selector: 'app-audit-log-filter-bar',
  imports: [ReactiveFormsModule, HlmInputImports, HlmSelectImports, HlmIconImports],
  providers: [provideIcons({ lucideSearch })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'audit-log-filter-bar.html',
})
export class AuditLogFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input<AuditLogFilterValue>({
    search: '',
    action: '',
    tableName: '',
    fromDate: '',
    toDate: '',
  });

  readonly filterChanged = output<AuditLogFilterValue>();

  protected readonly actions = AuditAction;

  protected readonly actionLabel = (value: AuditAction | ''): string => {
    switch (value) {
      case AuditAction.INSERT:
        return 'Insert';
      case AuditAction.UPDATE:
        return 'Update';
      case AuditAction.DELETE:
        return 'Delete';
      case AuditAction.OVERRIDE:
        return 'Override';
      default:
        return 'All Actions';
    }
  };

  readonly form = this._fb.nonNullable.group({
    search: '',
    action: '' as AuditAction | '',
    tableName: '',
    fromDate: '',
    toDate: '',
  });

  ngOnInit(): void {
    this.form.patchValue(this.initial(), { emitEvent: false });

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.tableName.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.action.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.fromDate.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.toDate.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    this.filterChanged.emit(this.form.getRawValue());
  }
}
