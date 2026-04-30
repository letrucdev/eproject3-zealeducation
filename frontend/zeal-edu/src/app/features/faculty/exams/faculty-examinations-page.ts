import { ChangeDetectionStrategy, Component, computed, inject, signal, viewChild } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideSearch } from '@ng-icons/lucide';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { DataTableSortChange } from '@shared/components/data-table';
import { FacultyService } from '../faculty.service';
import {
  FacultyExaminationCandidate,
  FacultyExaminationSummary,
  FacultyExaminationsQuery,
} from '../models/faculty-models';
import {
  FacultyExamCandidatesDialog,
} from './components/faculty-exam-candidates-dialog';
import {
  FacultyExamScoreFormDialog,
  FacultyExamScoreSubmit,
} from './components/faculty-exam-score-form-dialog';
import { FacultyExamTable } from './components/faculty-exam-table';

@Component({
  selector: 'app-faculty-examinations-page',
  imports: [
    ReactiveFormsModule,
    HlmCardImports,
    HlmIconImports,
    HlmInputImports,
    FacultyExamTable,
    FacultyExamCandidatesDialog,
    FacultyExamScoreFormDialog,
  ],
  providers: [provideIcons({ lucideSearch })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="flex flex-col gap-6">
      <div>
        <h1 class="text-2xl font-semibold tracking-tight">Examinations</h1>
        <p class="text-muted-foreground text-sm">
          Examinations for active batches assigned to you.
        </p>
      </div>

      <section hlmCard>
        <div hlmCardContent class="flex flex-col gap-4">
          <div class="relative w-full md:w-96">
            <ng-icon
              hlm
              name="lucideSearch"
              size="sm"
              class="text-muted-foreground pointer-events-none absolute inset-y-0 inset-s-3 my-auto"
            />
            <input
              hlmInput
              type="search"
              class="w-full ps-9"
              placeholder="Search by exam name or location..."
              [formControl]="searchControl"
              aria-label="Search examinations"
            />
          </div>

          <app-faculty-exam-table
            [page]="listQuery.data()"
            [isLoading]="listQuery.isPending()"
            [pageSize]="pageSize()"
            [sortBy]="sortBy()"
            [sortDirection]="sortDirection()"
            (enterScoresClicked)="onEnterScores($event)"
            (pageChanged)="onPageChanged($event)"
            (pageSizeChanged)="onPageSizeChanged($event)"
            (sortChanged)="onSortChanged($event)"
          />
        </div>
      </section>
    </div>

    <app-faculty-exam-candidates-dialog
      #candidatesDialog
      [examination]="activeExamination()"
      [candidates]="candidatesQuery.data()"
      [isLoading]="candidatesQuery.isPending() || candidatesQuery.isFetching()"
      (rowSelected)="onCandidateSelected($event)"
    />

    <app-faculty-exam-score-form-dialog
      #scoreDialog
      [submitting]="
        createExamResultMutation.isPending() || updateExamResultMutation.isPending()
      "
      (submitted)="onScoreSubmitted($event)"
    />
  `,
})
export default class FacultyExaminationsPage {
  private readonly _service = inject(FacultyService);

  protected readonly candidatesDialog = viewChild.required<FacultyExamCandidatesDialog>('candidatesDialog');
  protected readonly scoreDialog = viewChild.required<FacultyExamScoreFormDialog>('scoreDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');

  protected readonly searchControl = new FormControl<string>('', { nonNullable: true });

  protected readonly activeExamination = signal<FacultyExaminationSummary | null>(null);
  private readonly _activeExaminationId = computed<string | null>(
    () => this.activeExamination()?.examinationId ?? null,
  );
  private readonly _candidatesEnabled = computed(() => this._activeExaminationId() !== null);

  private readonly _params = computed<FacultyExaminationsQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.examinationsQuery(this._params);
  protected readonly candidatesQuery = this._service.examCandidatesQuery(
    this._activeExaminationId,
    this._candidatesEnabled,
  );

  protected readonly createExamResultMutation = this._service.createExamResultMutation();
  protected readonly updateExamResultMutation = this._service.updateExamResultMutation();

  constructor() {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((value) => {
        this.search.set(value);
        this.page.set(1);
      });
  }

  protected onPageChanged(p: number): void {
    this.page.set(p);
  }

  protected onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  protected onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }

  protected onEnterScores(exam: FacultyExaminationSummary): void {
    this.activeExamination.set(exam);
    this.candidatesDialog().open();
  }

  protected onCandidateSelected(candidate: FacultyExaminationCandidate): void {
    const exam = this.activeExamination();
    if (!exam) return;
    this.scoreDialog().open(exam, candidate);
  }

  protected onScoreSubmitted(event: FacultyExamScoreSubmit): void {
    const successMessage = event.isFinalized ? 'Score finalized.' : 'Score saved as draft.';
    if (event.candidate.resultId) {
      this.updateExamResultMutation.mutate(
        {
          resultId: event.candidate.resultId,
          payload: { score: event.score, isFinalized: event.isFinalized },
        },
        {
          onSuccess: () => {
            toast.success(successMessage);
            this.scoreDialog().close();
          },
        },
      );
    } else {
      this.createExamResultMutation.mutate(
        {
          examinationId: event.examinationId,
          payload: {
            enrollmentId: event.candidate.enrollmentId,
            score: event.score,
            isFinalized: event.isFinalized,
          },
        },
        {
          onSuccess: () => {
            toast.success(successMessage);
            this.scoreDialog().close();
          },
        },
      );
    }
  }
}
