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
import { lucideCalendarClock, lucidePlus, lucideSearch } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { HlmCheckboxImports } from '@spartan-ng/helm/checkbox';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { EnquirySource } from '../../../../core/models/enquiry-source';
import { EnquiryStatus } from '../../../../core/models/enquiry-status';
import { ENQUIRY_SOURCE_LABELS, ENQUIRY_STATUS_LABELS } from './enquiry-labels';

export interface EnquiryFilterValue {
  search: string;
  status: EnquiryStatus | '';
  source: EnquirySource | '';
  dueFollowUpOnly: boolean;
}

@Component({
  selector: 'app-enquiry-filter-bar',
  imports: [
    ReactiveFormsModule,
    HlmInputImports,
    HlmSelectImports,
    HlmButtonImports,
    HlmIconImports,
    HlmCheckboxImports,
  ],
  providers: [provideIcons({ lucideSearch, lucidePlus, lucideCalendarClock })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'enquiry-filter-bar.html',
})
export class EnquiryFilterBar implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly initial = input<EnquiryFilterValue>({
    search: '',
    status: '',
    source: '',
    dueFollowUpOnly: false,
  });

  readonly filterChanged = output<EnquiryFilterValue>();
  readonly createClicked = output<void>();

  protected readonly statuses = EnquiryStatus;
  protected readonly sources = EnquirySource;

  protected readonly statusLabel = (value: EnquiryStatus | ''): string =>
    value ? ENQUIRY_STATUS_LABELS[value] : 'All Status';

  protected readonly sourceLabel = (value: EnquirySource | ''): string =>
    value ? ENQUIRY_SOURCE_LABELS[value] : 'All Sources';

  readonly form = this._fb.nonNullable.group({
    search: '',
    status: '' as EnquiryStatus | '',
    source: '' as EnquirySource | '',
    dueFollowUpOnly: false,
  });

  ngOnInit(): void {
    this.form.patchValue(this.initial(), { emitEvent: false });

    this.form.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.status.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.source.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());

    this.form.controls.dueFollowUpOnly.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this._emit());
  }

  private _emit(): void {
    this.filterChanged.emit(this.form.getRawValue());
  }
}
