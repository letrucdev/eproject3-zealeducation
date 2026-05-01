import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideChevronLeft,
  lucideChevronRight,
  lucideFolderOpen,
  lucidePlus,
  lucideSearch,
} from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmAccordionImports } from '@spartan-ng/helm/accordion';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmCheckboxImports } from '@spartan-ng/helm/checkbox';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { ConfirmDialog } from '@shared/components/confirm-dialog/confirm-dialog';
import { MaterialCardAction } from '@shared/components/material-card-item/material-card-item';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CourseMaterialsSection } from './components/course-materials-section';
import { MaterialFormDialog, MaterialFormSubmit } from './components/material-form-dialog';
import { ReplaceFileDialog, ReplaceFileSubmit } from './components/replace-file-dialog';
import { MaterialsService } from './materials.service';
import { StudyMaterialListItem } from '@core/models/study-material';
import { CourseListQuery } from './models/material-payload';

@Component({
  selector: 'app-materials-management-page',
  imports: [
    ReactiveFormsModule,
    HlmAccordionImports,
    HlmButtonImports,
    HlmCardImports,
    HlmCheckboxImports,
    HlmIconImports,
    HlmInputImports,
    HlmSpinnerImports,

    NgIcon,
    CourseMaterialsSection,
    MaterialFormDialog,
    ReplaceFileDialog,
    ConfirmDialog,
  ],
  providers: [
    provideIcons({
      lucideChevronLeft,
      lucideChevronRight,
      lucideFolderOpen,
      lucidePlus,
      lucideSearch,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'materials-management-page.html',
})
export default class MaterialsManagementPage implements OnInit {
  private readonly _service = inject(MaterialsService);
  private readonly _fb = inject(FormBuilder);
  private readonly _destroyRef = inject(DestroyRef);

  protected readonly formDialog = viewChild.required<MaterialFormDialog>('formDialog');
  protected readonly replaceDialog = viewChild.required<ReplaceFileDialog>('replaceDialog');
  protected readonly confirmDialog = viewChild.required<ConfirmDialog>('confirmDialog');

  protected readonly search = signal('');
  protected readonly includeInactive = signal(true);
  protected readonly coursePage = signal(1);
  protected readonly coursePageSize = signal(10);
  private readonly _pendingDeleteId = signal<string | null>(null);

  protected readonly filterForm = this._fb.nonNullable.group({
    search: '',
    includeInactive: true,
  });

  private readonly _coursesParams = computed<CourseListQuery>(() => ({
    page: this.coursePage(),
    pageSize: this.coursePageSize(),
    search: this.search() || undefined,
  }));

  protected readonly coursesQuery = this._service.listCoursesQuery(this._coursesParams);

  protected readonly createMutation = this._service.createMutation();
  protected readonly updateTitleMutation = this._service.updateTitleMutation();
  protected readonly replaceFileMutation = this._service.replaceFileMutation();
  protected readonly toggleActiveMutation = this._service.toggleActiveMutation();
  protected readonly deleteMutation = this._service.deleteMutation();

  protected readonly canPrevCoursePage = computed(
    () => this.coursesQuery.data()?.hasPreviousPage ?? false,
  );
  protected readonly canNextCoursePage = computed(
    () => this.coursesQuery.data()?.hasNextPage ?? false,
  );

  ngOnInit(): void {
    this.filterForm.controls.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        this.search.set((value ?? '').trim());
        this.coursePage.set(1);
      });
    this.filterForm.controls.includeInactive.valueChanges
      .pipe(distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((value) => {
        this.includeInactive.set(!!value);
      });
  }

  protected onUploadClick(): void {
    this.formDialog().openCreate();
  }

  protected onCardAction(event: MaterialCardAction<StudyMaterialListItem>): void {
    switch (event.kind) {
      case 'download':
        void this._download(event.material);
        return;
      case 'edit-title':
        this.formDialog().openEditTitle(event.material);
        return;
      case 'replace-file':
        this.replaceDialog().open(event.material);
        return;
      case 'toggle-active':
        this._toggleActive(event.material);
        return;
      case 'delete':
        void this._confirmDelete(event.material);
        return;
    }
  }

  protected onFormSubmitted(event: MaterialFormSubmit): void {
    if (this.createMutation.isPending() || this.updateTitleMutation.isPending()) return;

    if (event.mode === 'create') {
      this.createMutation.mutate(event.payload, {
        onSuccess: () => {
          toast.success('Material uploaded successfully.');
          this.formDialog().close();
        },
      });
    } else {
      this.updateTitleMutation.mutate(
        { materialId: event.materialId, payload: event.payload },
        {
          onSuccess: () => {
            toast.success('Title updated.');
            this.formDialog().close();
          },
        },
      );
    }
  }

  protected onReplaceSubmitted(event: ReplaceFileSubmit): void {
    this.replaceFileMutation.mutate(event, {
      onSuccess: () => {
        toast.success('File replaced.');
        this.replaceDialog().close();
      },
    });
  }

  protected goPrevCoursePage(): void {
    if (this.canPrevCoursePage()) {
      this.coursePage.update((p) => Math.max(1, p - 1));
    }
  }

  protected goNextCoursePage(): void {
    if (this.canNextCoursePage()) {
      this.coursePage.update((p) => p + 1);
    }
  }

  protected onConfirmDelete(): void {
    const id = this._pendingDeleteId();
    if (!id) return;
    this.deleteMutation.mutate(id, {
      onSuccess: () => {
        toast.success('Material deleted.');
      },
      onSettled: () => this._pendingDeleteId.set(null),
    });
  }

  private async _download(material: StudyMaterialListItem): Promise<void> {
    try {
      const { blob, fileName } = await this._service.downloadFile(material.materialId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName || material.fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {}
  }

  private _toggleActive(material: StudyMaterialListItem): void {
    this.toggleActiveMutation.mutate(
      { materialId: material.materialId, payload: { isActive: !material.isActive } },
      {
        onSuccess: () => {
          toast.success(material.isActive ? 'Material hidden.' : 'Material shown.');
        },
      },
    );
  }

  private async _confirmDelete(material: StudyMaterialListItem): Promise<void> {
    this._pendingDeleteId.set(material.materialId);

    let message = `Are you sure you want to permanently delete "${material.title}"? This cannot be undone.`;
    try {
      const siblings = await this._service.getSiblings(material.materialId);
      const otherCourses = siblings
        .filter((s) => s.materialId !== material.materialId)
        .map((s) => s.courseName);
      if (otherCourses.length > 0) {
        const courseList = otherCourses.map((name) => `• ${name}`).join('\n');
        message =
          `"${material.title}" shares the same file with ${otherCourses.length} other ` +
          `course${otherCourses.length > 1 ? 's' : ''}:\n${courseList}\n\n` +
          `Deleting will permanently remove this material from ALL of those courses. ` +
          `This cannot be undone.`;
      }
    } catch {}

    this.confirmDialog().open({
      title: 'Delete material',
      message,
      confirmLabel: 'Delete',
      destructive: true,
    });
  }
}
