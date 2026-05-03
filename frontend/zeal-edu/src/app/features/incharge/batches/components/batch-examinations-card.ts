import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  Directive,
  OnInit,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCirclePlus,
  lucideListChecks,
  lucidePencil,
  lucideSearch,
  lucideTrash2,
} from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { Examination } from '@core/models/examination';
import { PaginatedList } from '@core/models/paginated-list';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';

@Directive({
  selector: '[examinationCell]',
  providers: [{ provide: DataTableCellDef, useExisting: ExaminationCellDef }],
})
export class ExaminationCellDef extends DataTableCellDef<Examination> {
  override readonly appDataTableCell = input.required<string>({ alias: 'examinationCell' });

  static override ngTemplateContextGuard(
    _dir: ExaminationCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<Examination> {
    return true;
  }
}

@Component({
  selector: 'app-batch-examinations-card',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    DataTable,
    ExaminationCellDef,
    HlmCardImports,
    HlmButtonImports,
    HlmIconImports,
    HlmInputImports,
  ],
  providers: [
    provideIcons({
      lucideCirclePlus,
      lucideListChecks,
      lucidePencil,
      lucideSearch,
      lucideTrash2,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'batch-examinations-card.html',
})
export class BatchExaminationsCard implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly page = input<PaginatedList<Examination> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly canMutate = input<boolean>(true);
  readonly canAddExamination = input<boolean>(true);
  readonly hasSchedule = input<boolean>(true);
  readonly pageSize = input<number>(10);
  readonly search = input<string>('');
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly addClicked = output<void>();
  readonly viewResultsClicked = output<Examination>();
  readonly editClicked = output<Examination>();
  readonly deleteClicked = output<Examination>();
  readonly searchChanged = output<string>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly searchControl = this._fb.nonNullable.control('');

  protected readonly columns: DataTableColumn<Examination>[] = [
    { key: 'examName', header: 'Exam Name', sortable: true },
    { key: 'examDate', header: 'Date', sortable: true },
    { key: 'location', header: 'Location', sortable: true },
    { key: 'maxScore', header: 'Max', sortable: true, align: 'center' },
    { key: 'passScore', header: 'Pass', sortable: true, align: 'center' },
    { key: 'scheduledByName', header: 'Scheduled By', align: 'center' },
    { key: 'actions', header: 'Actions', align: 'right' },
  ];

  protected readonly trackById = (row: Examination): string => row.examinationId;

  constructor() {
    effect(() => {
      const next = this.search();
      if (this.searchControl.value !== next) {
        this.searchControl.setValue(next, { emitEvent: false });
      }
    });
  }

  ngOnInit(): void {
    this.searchControl.setValue(this.search(), { emitEvent: false });
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => this.searchChanged.emit(value ?? ''));
  }
}
