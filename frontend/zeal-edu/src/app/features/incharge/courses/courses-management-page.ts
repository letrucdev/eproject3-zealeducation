import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { toast } from '@spartan-ng/brain/sonner';
import { CourseListItem } from '@core/models/course-list-item';
import { CourseListQuery, CoursesService } from '@core/services/courses.service';
import {
  CourseFilterBar,
  CourseFilterValue,
} from './components/course-filter-bar';
import {
  CourseFormDialog,
  CourseFormSubmit,
} from './components/course-form-dialog';
import { CourseStatsCards } from './components/course-stats-cards';
import { CourseTable } from './components/course-table';

@Component({
  selector: 'app-courses-management-page',
  imports: [CourseStatsCards, CourseFilterBar, CourseTable, CourseFormDialog],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'courses-management-page.html',
})
export default class CoursesManagementPage {
  private readonly _service = inject(CoursesService);

  protected readonly formDialog = viewChild.required<CourseFormDialog>('formDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly isActiveFilter = signal<boolean | null>(null);

  protected readonly initialFilter: CourseFilterValue = { search: '', isActive: null };

  protected readonly focusedCourseId = signal<string | null>(null);

  private readonly _listParams = computed<CourseListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    isActive: this.isActiveFilter() ?? undefined,
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly statsQuery = this._service.statisticsQuery();
  protected readonly detailQuery = this._service.detailQuery(this.focusedCourseId);
  protected readonly createMutation = this._service.createMutation();
  protected readonly updateMutation = this._service.updateMutation();

  protected readonly isSubmittingForm = computed(
    () => this.createMutation.isPending() || this.updateMutation.isPending(),
  );

  constructor() {
    effect(() => {
      const detail = this.detailQuery.data();
      const currentId = this.focusedCourseId();
      if (!detail || currentId !== detail.courseId) return;

      untracked(() => {
        this.formDialog().openEdit(detail);
        this.focusedCourseId.set(null);
      });
    });

    effect(() => {
      const error = this.detailQuery.error();
      if (!error) return;
      untracked(() => this.focusedCourseId.set(null));
    });
  }

  onFilterChanged(value: CourseFilterValue): void {
    this.search.set(value.search);
    this.isActiveFilter.set(value.isActive);
    this.page.set(1);
  }

  onPageChanged(page: number): void {
    this.page.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  onCreateClicked(): void {
    this.formDialog().openCreate();
  }

  onEditClicked(row: CourseListItem): void {
    this.focusedCourseId.set(row.courseId);
  }

  onFormSubmitted(event: CourseFormSubmit): void {
    if (event.mode === 'create') {
      this.createMutation.mutate(event.payload, {
        onSuccess: () => {
          toast.success('Course created successfully.');
          this.formDialog().close();
        },
      });
    } else {
      this.updateMutation.mutate(
        { courseId: event.courseId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Course updated successfully.');
            this.formDialog().close();
          },
        },
      );
    }
  }
}
