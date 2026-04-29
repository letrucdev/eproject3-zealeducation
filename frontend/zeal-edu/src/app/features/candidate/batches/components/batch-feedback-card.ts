import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideCircleCheck, lucideMessageSquare } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { MyBatchDetail } from '../../models/candidate-portal-models';

@Component({
  selector: 'app-candidate-batch-feedback-card',
  imports: [HlmCardImports, HlmButtonImports, HlmBadgeImports, HlmIconImports],
  providers: [provideIcons({ lucideCircleCheck, lucideMessageSquare })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader>
        <h3 hlmCardTitle>Feedback</h3>
        <p hlmCardDescription>
          Share your experience with this batch. Each feedback can be submitted once.
        </p>
      </div>
      <div hlmCardContent class="grid gap-4 md:grid-cols-3">
        <div class="flex flex-col gap-2 rounded-md border p-4">
          <div class="flex items-center justify-between gap-2">
            <span class="text-sm font-medium">Faculty</span>
            @if (facultySubmitted()) {
              <span hlmBadge class="bg-emerald-100 text-emerald-800">
                <ng-icon hlm name="lucideCircleCheck" size="sm" class="mr-1" />
                Submitted
              </span>
            }
          </div>
          <p class="text-muted-foreground text-xs">
            Rate the instructor of this batch.
          </p>
          <button
            hlmBtn
            variant="outline"
            size="sm"
            type="button"
            class="self-start"
            [disabled]="!detail().facultyId || facultySubmitted()"
            (click)="facultyClicked.emit()"
          >
            <ng-icon hlm name="lucideMessageSquare" size="sm" />
            {{ facultySubmitted() ? 'Already submitted' : 'Submit faculty feedback' }}
          </button>
        </div>

        <div class="flex flex-col gap-2 rounded-md border p-4">
          <div class="flex items-center justify-between gap-2">
            <span class="text-sm font-medium">Course</span>
            @if (detail().feedbackState.courseSubmitted) {
              <span hlmBadge class="bg-emerald-100 text-emerald-800">
                <ng-icon hlm name="lucideCircleCheck" size="sm" class="mr-1" />
                Submitted
              </span>
            }
          </div>
          <p class="text-muted-foreground text-xs">Rate the course curriculum and content.</p>
          <button
            hlmBtn
            variant="outline"
            size="sm"
            type="button"
            class="self-start"
            [disabled]="detail().feedbackState.courseSubmitted"
            (click)="courseClicked.emit()"
          >
            <ng-icon hlm name="lucideMessageSquare" size="sm" />
            {{
              detail().feedbackState.courseSubmitted
                ? 'Already submitted'
                : 'Submit course feedback'
            }}
          </button>
        </div>

        <div class="flex flex-col gap-2 rounded-md border p-4">
          <div class="flex items-center justify-between gap-2">
            <span class="text-sm font-medium">General</span>
            @if (detail().feedbackState.generalSubmitted) {
              <span hlmBadge class="bg-emerald-100 text-emerald-800">
                <ng-icon hlm name="lucideCircleCheck" size="sm" class="mr-1" />
                Submitted
              </span>
            }
          </div>
          <p class="text-muted-foreground text-xs">Share your overall experience.</p>
          <button
            hlmBtn
            variant="outline"
            size="sm"
            type="button"
            class="self-start"
            [disabled]="detail().feedbackState.generalSubmitted"
            (click)="generalClicked.emit()"
          >
            <ng-icon hlm name="lucideMessageSquare" size="sm" />
            {{
              detail().feedbackState.generalSubmitted
                ? 'Already submitted'
                : 'Submit general feedback'
            }}
          </button>
        </div>
      </div>
    </section>
  `,
})
export class CandidateBatchFeedbackCard {
  readonly detail = input.required<MyBatchDetail>();

  readonly facultyClicked = output<void>();
  readonly courseClicked = output<void>();
  readonly generalClicked = output<void>();

  protected readonly facultySubmitted = computed(() => {
    const detail = this.detail();
    if (!detail.facultyId) return false;
    return detail.feedbackState.facultyTargetsSubmitted.includes(detail.facultyId);
  });
}
