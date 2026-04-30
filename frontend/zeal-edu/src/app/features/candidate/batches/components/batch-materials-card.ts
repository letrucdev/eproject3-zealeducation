import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideChevronLeft, lucideChevronRight, lucideInbox } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { PaginatedList } from '@core/models/paginated-list';
import {
  MaterialCardAction,
  MaterialCardItem,
} from '@shared/components/material-card-item/material-card-item';
import { StudyMaterialListItem } from '@features/incharge/materials/models/material-payload';

@Component({
  selector: 'app-candidate-batch-materials-card',
  imports: [HlmButtonImports, HlmCardImports, HlmSpinnerImports, MaterialCardItem, NgIcon],
  providers: [provideIcons({ lucideChevronLeft, lucideChevronRight, lucideInbox })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @let p = data();
    @let items = p?.items ?? [];
    <section hlmCard>
      <div hlmCardHeader>
        <h3 hlmCardTitle>Study Materials</h3>
        <p hlmCardDescription>Materials for this course. View or download.</p>
      </div>
      <div hlmCardContent>
        @if (isLoading() && !p) {
          <div class="flex items-center justify-center py-8">
            <hlm-spinner />
          </div>
        } @else if (items.length === 0) {
          <div
            class="text-muted-foreground flex flex-col items-center justify-center gap-2 rounded-md border border-dashed py-8 text-sm"
          >
            <ng-icon name="lucideInbox" class="text-[24px]" />
            <p>No materials available for this course yet.</p>
          </div>
        } @else {
          <div
            class="grid grid-cols-1 gap-3 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5"
          >
            @for (m of items; track m.materialId) {
              <app-material-card-item
                [material]="m"
                [readOnly]="true"
                (action)="onCardAction($event)"
              />
            }
          </div>

          @if (p && p.totalPages > 1) {
            <div class="text-muted-foreground mt-4 flex items-center justify-end gap-2 text-xs">
              <span>Page {{ p.pageNumber }} / {{ p.totalPages }}</span>
              <button
                hlmBtn
                variant="outline"
                size="icon-sm"
                type="button"
                [disabled]="!canPrev()"
                aria-label="Previous page"
                (click)="onPrev()"
              >
                <ng-icon name="lucideChevronLeft" class="text-[16px]" />
              </button>
              <button
                hlmBtn
                variant="outline"
                size="icon-sm"
                type="button"
                [disabled]="!canNext()"
                aria-label="Next page"
                (click)="onNext()"
              >
                <ng-icon name="lucideChevronRight" class="text-[16px]" />
              </button>
            </div>
          }
        }
      </div>
    </section>
  `,
})
export class CandidateBatchMaterialsCard {
  readonly data = input<PaginatedList<StudyMaterialListItem> | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  readonly pageChanged = output<number>();
  readonly downloadClicked = output<StudyMaterialListItem>();

  protected readonly canPrev = computed(() => this.data()?.hasPreviousPage ?? false);
  protected readonly canNext = computed(() => this.data()?.hasNextPage ?? false);

  protected onCardAction(event: MaterialCardAction): void {
    if (event.kind === 'download') {
      this.downloadClicked.emit(event.material as StudyMaterialListItem);
    }
  }

  protected onPrev(): void {
    const current = this.data()?.pageNumber ?? 1;
    if (this.canPrev()) this.pageChanged.emit(Math.max(1, current - 1));
  }

  protected onNext(): void {
    const current = this.data()?.pageNumber ?? 1;
    if (this.canNext()) this.pageChanged.emit(current + 1);
  }
}
