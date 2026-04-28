import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  viewChild,
} from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucidePencil, lucidePlus } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { FacultyExaminationCandidate, FacultyExaminationSummary } from '../../models/faculty-models';

@Component({
  selector: 'app-faculty-exam-candidates-dialog',
  imports: [
    DatePipe,
    HlmBadgeImports,
    HlmButtonImports,
    HlmDialogImports,
    HlmIconImports,
    HlmSkeletonImports,
  ],
  providers: [provideIcons({ lucidePlus, lucidePencil })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-3xl w-3xl flex max-h-[90dvh] flex-col"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          @if (examination(); as exam) {
            <h2 hlmDialogTitle>{{ exam.examName }}</h2>
            <p class="text-muted-foreground text-sm">
              {{ exam.batchCode }} — {{ exam.courseName }} · Max {{ exam.maxScore }} · Pass
              {{ exam.passScore }}
            </p>
          } @else {
            <h2 hlmDialogTitle>Exam Candidates</h2>
          }
        </div>

        <div class="min-h-0 flex-1 overflow-y-auto overflow-x-hidden">
          @if (isLoading()) {
            <hlm-skeleton class="h-12 w-full" />
            <hlm-skeleton class="my-2 h-12 w-full" />
            <hlm-skeleton class="h-12 w-full" />
          } @else if ((candidates()?.length ?? 0) === 0) {
            <p class="text-muted-foreground py-6 text-center text-sm">
              No candidates enrolled in this batch yet.
            </p>
          } @else {
            <table class="w-full text-sm">
              <thead class="text-muted-foreground border-b text-left text-xs uppercase">
                <tr>
                  <th class="px-3 py-2 w-32">Code</th>
                  <th class="px-3 py-2">Candidate</th>
                  <th class="px-3 py-2 w-20 text-center">Score</th>
                  <th class="px-3 py-2 w-20 text-center">Grade</th>
                  <th class="px-3 py-2 w-24 text-center">Status</th>
                  <th class="px-3 py-2 w-40">Graded At</th>
                  <th class="px-3 py-2 w-24 text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (row of candidates(); track row.enrollmentId) {
                  <tr class="border-b">
                    <td class="px-3 py-2 font-mono">{{ row.candidateCode }}</td>
                    <td class="px-3 py-2 font-medium">{{ row.candidateFullName }}</td>
                    <td class="px-3 py-2 text-center">
                      {{ row.score != null ? row.score : '–' }}
                    </td>
                    <td class="px-3 py-2 text-center">{{ row.grade || '–' }}</td>
                    <td class="px-3 py-2 text-center">
                      @if (row.resultId == null) {
                        <span class="text-muted-foreground text-xs">Not entered</span>
                      } @else if (row.isPassed) {
                        <span hlmBadge class="bg-emerald-100 text-emerald-800">Pass</span>
                      } @else {
                        <span hlmBadge class="bg-rose-100 text-rose-800">Fail</span>
                      }
                      @if (row.isOverridden) {
                        <span hlmBadge class="ml-1 bg-amber-100 text-amber-800">Overridden</span>
                      }
                    </td>
                    <td class="px-3 py-2 text-xs">
                      @if (row.gradedAt) {
                        {{ row.gradedAt | date: 'dd MMM yyyy, HH:mm' }}
                      } @else {
                        –
                      }
                    </td>
                    <td class="px-3 py-2 text-right">
                      <button
                        hlmBtn
                        variant="ghost"
                        size="sm"
                        type="button"
                        (click)="rowSelected.emit(row)"
                        [disabled]="row.isOverridden"
                        [attr.title]="
                          row.isOverridden
                            ? 'Result has been overridden by Incharge'
                            : row.resultId
                              ? 'Update score'
                              : 'Enter score'
                        "
                        [attr.aria-label]="
                          (row.resultId ? 'Edit score for ' : 'Add score for ') + row.candidateCode
                        "
                      >
                        @if (row.resultId == null) {
                          <ng-icon hlm name="lucidePlus" size="sm" />
                        } @else {
                          <ng-icon hlm name="lucidePencil" size="sm" />
                        }
                      </button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        <div hlmDialogFooter class="shrink-0">
          <p class="text-muted-foreground mr-auto text-xs">
            Graded: {{ gradedCount() }} / {{ candidates()?.length ?? 0 }}
          </p>
          <button hlmBtn variant="outline" type="button" hlmDialogClose>Close</button>
        </div>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class FacultyExamCandidatesDialog {
  readonly examination = input<FacultyExaminationSummary | null>(null);
  readonly candidates = input<FacultyExaminationCandidate[] | null | undefined>([]);
  readonly isLoading = input<boolean>(false);

  readonly rowSelected = output<FacultyExaminationCandidate>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  protected readonly gradedCount = computed(
    () => this.candidates()?.filter((c) => c.resultId != null).length ?? 0,
  );

  open(): void {
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }
}
