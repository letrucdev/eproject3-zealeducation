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
import { lucideSearch } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { BatchEnrollmentItem } from '@core/models/batch-enrollment';
import { PaginatedList } from '@core/models/paginated-list';
import { EnrollmentStatus } from '@features/incharge/candidates/models/candidate-detail';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
  DataTableSortChange,
  SortDirection,
} from '@shared/components/data-table';

@Directive({
  selector: '[facultyEnrollmentCell]',
  providers: [{ provide: DataTableCellDef, useExisting: FacultyEnrollmentCellDef }],
})
export class FacultyEnrollmentCellDef extends DataTableCellDef<BatchEnrollmentItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'facultyEnrollmentCell' });

  static override ngTemplateContextGuard(
    _dir: FacultyEnrollmentCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<BatchEnrollmentItem> {
    return true;
  }
}

@Component({
  selector: 'app-faculty-batch-enrollments-table',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    DataTable,
    FacultyEnrollmentCellDef,
    HlmBadgeImports,
    HlmCardImports,
    HlmIconImports,
    HlmInputImports,
  ],
  providers: [provideIcons({ lucideSearch })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader class="flex flex-col gap-4">
        <div class="flex flex-col gap-1">
          <h3 hlmCardTitle>Enrolled Candidates</h3>
          <p hlmCardDescription>
            @if (page(); as p) {
              {{ p.totalCount }} candidate(s) currently enrolled in this batch.
            } @else {
              Candidates enrolled in this batch.
            }
          </p>
        </div>
        <div class="relative w-full sm:max-w-xs">
          <ng-icon
            hlm
            name="lucideSearch"
            size="sm"
            class="text-muted-foreground absolute left-3 top-1/2 -translate-y-1/2"
          />
          <input
            hlmInput
            type="search"
            [formControl]="searchControl"
            placeholder="Search by code, name, email or phone"
            aria-label="Search enrolled candidates"
            class="w-full pl-9"
          />
        </div>
      </div>
      <div hlmCardContent>
        <app-data-table
          [columns]="columns"
          [page]="page()"
          [isLoading]="isLoading()"
          [pageSize]="pageSize()"
          [sortBy]="sortBy()"
          [sortDirection]="sortDirection()"
          [trackBy]="trackById"
          itemLabel="candidates"
          emptyMessage="No candidates have been enrolled in this batch yet."
          (pageChanged)="pageChanged.emit($event)"
          (pageSizeChanged)="pageSizeChanged.emit($event)"
          (sortChanged)="sortChanged.emit($event)"
        >
          <ng-template facultyEnrollmentCell="candidateCode" let-row>
            <span class="font-mono">{{ row.candidateCode }}</span>
          </ng-template>

          <ng-template facultyEnrollmentCell="fullName" let-row>
            <span class="font-medium">{{ row.fullName }}</span>
          </ng-template>

          <ng-template facultyEnrollmentCell="contact" let-row>
            <div class="flex flex-col text-xs">
              <span>{{ row.email }}</span>
              <span class="text-muted-foreground">{{ row.phone }}</span>
            </div>
          </ng-template>

          <ng-template facultyEnrollmentCell="enrollmentDate" let-row>
            {{ row.enrollmentDate | date: 'dd MMM yyyy' }}
          </ng-template>

          <ng-template facultyEnrollmentCell="status" let-row>
            @switch (row.status) {
              @case (enrollmentStatuses.Enrolled) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Enrolled</span>
              }
              @case (enrollmentStatuses.Completed) {
                <span hlmBadge class="bg-sky-100 text-sky-800">Completed</span>
              }
              @case (enrollmentStatuses.Withdrawn) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Withdrawn</span>
              }
              @case (enrollmentStatuses.OnBreak) {
                <span hlmBadge class="bg-slate-100 text-slate-800">On Break</span>
              }
              @default {
                <span hlmBadge class="bg-amber-100 text-amber-800">{{ row.status }}</span>
              }
            }
          </ng-template>
        </app-data-table>
      </div>
    </section>
  `,
})
export class FacultyBatchEnrollmentsTable implements OnInit {
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  readonly page = input<PaginatedList<BatchEnrollmentItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);
  readonly pageSize = input<number>(10);
  readonly search = input<string>('');
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection | null>(null);

  readonly searchChanged = output<string>();
  readonly pageChanged = output<number>();
  readonly pageSizeChanged = output<number>();
  readonly sortChanged = output<DataTableSortChange>();

  protected readonly enrollmentStatuses = EnrollmentStatus;

  protected readonly searchControl = this._fb.nonNullable.control('');

  protected readonly columns: DataTableColumn<BatchEnrollmentItem>[] = [
    { key: 'candidateCode', header: 'Code', sortable: true, width: 'w-32' },
    { key: 'fullName', header: 'Full Name', sortable: true, width: 'w-56' },
    { key: 'contact', header: 'Contact', width: 'w-64' },
    { key: 'enrollmentDate', header: 'Enrolled On', sortable: true, width: 'w-40' },
    { key: 'status', header: 'Status', sortable: true, align: 'center', width: 'w-32' },
  ];

  protected readonly trackById = (row: BatchEnrollmentItem): string => row.enrollmentId;

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
