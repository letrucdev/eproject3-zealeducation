import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, input, output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AssetConditionStatus } from '../../../../core/models/asset-condition-status';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucidePlus, lucideSearch } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';

export interface AssetFilterValue {
  search: string;
  conditionStatus: AssetConditionStatus | '';
}

@Component({
  selector: 'app-asset-filter-bar',
  standalone: true,
  imports: [ReactiveFormsModule, HlmInputImports, HlmSelectImports, HlmButtonImports, NgIcon, HlmIconImports],
  providers: [provideIcons({ lucidePlus, lucideSearch })],
  templateUrl: './asset-filter-bar.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export default class AssetFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input.required<AssetFilterValue>();
  readonly showingDecommissioned = input<boolean>(false);

  readonly filterChanged = output<AssetFilterValue>();
  readonly createClicked = output<void>();

  protected readonly form = this._fb.nonNullable.group({
    search: [''],
    conditionStatus: ['' as AssetConditionStatus | ''],
  });


  protected readonly statusLabel = (item: AssetConditionStatus | '') => {
    return item === '' ? 'All Status' : item;
  };

  ngOnInit() {
    this.form.patchValue(this.initial(), { emitEvent: false });

    this.form.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)), takeUntilDestroyed(this._destroyRef))
      .subscribe((val) => {
        this.filterChanged.emit(val as AssetFilterValue);
      });
  }
}
