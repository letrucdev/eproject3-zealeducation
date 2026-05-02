import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideChevronLeft, lucideChevronRight, lucideInbox } from '@ng-icons/lucide';
import { HlmAccordionImports } from '@spartan-ng/helm/accordion';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { MaterialCardAction, MaterialCardItem } from '@shared/components/material-card-item/material-card-item';
import { MaterialsService } from '../materials.service';
import { CourseMaterialsQuery, StudyMaterialListItem } from '@core/models/study-material';
import { MaterialCourseListItem } from '../models/material-payload';
import { AutoSizeAccordionContent } from './auto-size-accordion-content';

@Component({
  selector: 'hlm-accordion-item[appCourseMaterialsSection]',
  imports: [
    HlmAccordionImports,
    HlmButtonImports,
    HlmSpinnerImports,
    MaterialCardItem,
    AutoSizeAccordionContent,
    NgIcon,
  ],
  providers: [provideIcons({ lucideChevronLeft, lucideChevronRight, lucideInbox })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'course-materials-section.html',
  host: {
    '(openedChange)': 'onOpenedChange($any($event))',
  },
})
export class CourseMaterialsSection {
  private readonly _service = inject(MaterialsService);

  readonly course = input.required<MaterialCourseListItem>();
  readonly search = input<string>('');
  readonly includeInactive = input<boolean>(true);
  readonly action = output<MaterialCardAction<StudyMaterialListItem>>();

  protected readonly isOpened = signal(false);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(12);

  private readonly _courseId = computed(() => this.course().courseId);
  private readonly _params = computed<CourseMaterialsQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    includeInactive: this.includeInactive(),
  }));

  protected readonly materialsQuery = this._service.listCourseMaterialsQuery(
    this._courseId,
    this._params,
    this.isOpened,
  );

  protected readonly canPrev = computed(
    () => this.materialsQuery.data()?.hasPreviousPage ?? false,
  );
  protected readonly canNext = computed(() => this.materialsQuery.data()?.hasNextPage ?? false);

  constructor() {
    effect(() => {
      this.search();
      this.includeInactive();
      this.page.set(1);
    });
  }

  protected onOpenedChange(opened: boolean): void {
    this.isOpened.set(opened);
  }

  protected onCardAction(event: MaterialCardAction): void {
    this.action.emit(event as MaterialCardAction<StudyMaterialListItem>);
  }

  protected goPrev(): void {
    if (this.canPrev()) {
      this.page.update((p) => Math.max(1, p - 1));
    }
  }

  protected goNext(): void {
    if (this.canNext()) {
      this.page.update((p) => p + 1);
    }
  }
}
