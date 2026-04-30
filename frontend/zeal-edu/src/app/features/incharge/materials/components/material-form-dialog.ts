import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideUpload, lucideX } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { CourseListItem } from '@core/models/course-list-item';
import { CoursesService } from '@core/services/courses.service';
import { StudyMaterialListItem } from '@core/models/study-material';
import {
  CreateMaterialsPayload,
  UpdateMaterialTitlePayload,
} from '../models/material-payload';

export type MaterialFormMode = 'create' | 'edit-title';

export interface MaterialFormSubmitCreate {
  mode: 'create';
  payload: CreateMaterialsPayload;
}

export interface MaterialFormSubmitEditTitle {
  mode: 'edit-title';
  materialId: string;
  payload: UpdateMaterialTitlePayload;
}

export type MaterialFormSubmit = MaterialFormSubmitCreate | MaterialFormSubmitEditTitle;

const MAX_FILE_BYTES = 50 * 1024 * 1024;

const ALLOWED_EXTENSIONS = [
  '.pdf',
  '.doc',
  '.docx',
  '.xls',
  '.xlsx',
  '.ppt',
  '.pptx',
  '.mp4',
  '.mov',
  '.jpg',
  '.jpeg',
  '.png',
];

const fileSizeValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const file = control.value as File | null;
  if (!file) return null;
  return file.size > MAX_FILE_BYTES ? { fileTooLarge: true } : null;
};

const fileExtensionValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const file = control.value as File | null;
  if (!file) return null;
  const lower = file.name.toLowerCase();
  return ALLOWED_EXTENSIONS.some((ext) => lower.endsWith(ext)) ? null : { fileTypeInvalid: true };
};

const atLeastOneCourseValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value = control.value as CourseListItem[] | null;
  return Array.isArray(value) && value.length > 0 ? null : { required: true };
};

@Component({
  selector: 'app-material-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmButtonImports,
    HlmComboboxImports,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmSpinnerImports,
    NgIcon,
  ],
  providers: [provideIcons({ lucideUpload, lucideX })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'material-form-dialog.html',
})
export class MaterialFormDialog {
  private readonly _fb = inject(FormBuilder);
  private readonly _coursesService = inject(CoursesService);

  readonly submitted = output<MaterialFormSubmit>();
  readonly submitting = input(false);

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  protected readonly mode = signal<MaterialFormMode>('create');
  protected readonly editingMaterial = signal<StudyMaterialListItem | null>(null);
  protected readonly isCreate = computed(() => this.mode() === 'create');
  protected readonly courseSearch = signal('');

  protected readonly form = this._fb.group({
    title: this._fb.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
    courses: this._fb.control<CourseListItem[]>([], [atLeastOneCourseValidator]),
    file: this._fb.control<File | null>(null, [
      Validators.required,
      fileSizeValidator,
      fileExtensionValidator,
    ]),
  });

  protected readonly courses = this._coursesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 100,
      search: this.courseSearch(),
      isActive: true,
    })),
  );

  protected readonly courseItemToString = (course: CourseListItem | null): string =>
    course?.courseName ?? '';

  protected readonly courseEqualsValue = (
    a: CourseListItem | null,
    b: CourseListItem | null,
  ): boolean => a?.courseId === b?.courseId;

  constructor() {
    effect(() => {
      if (this.isCreate()) return;
      // In edit-title mode, courses + file are not relevant.
      this.form.controls.courses.disable({ emitEvent: false });
      this.form.controls.file.disable({ emitEvent: false });
    });
  }

  openCreate(): void {
    this.mode.set('create');
    this.editingMaterial.set(null);
    this.form.reset({ title: '', courses: [], file: null });
    this.form.controls.courses.enable({ emitEvent: false });
    this.form.controls.file.enable({ emitEvent: false });
    this.dlg()?.open();
  }

  openEditTitle(material: StudyMaterialListItem): void {
    this.mode.set('edit-title');
    this.editingMaterial.set(material);
    this.form.reset({ title: material.title, courses: [], file: null });
    this.form.controls.courses.disable({ emitEvent: false });
    this.form.controls.file.disable({ emitEvent: false });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.form.controls.file.setValue(file);
    this.form.controls.file.markAsTouched();
    input.value = '';
  }

  protected onFileDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files?.[0] ?? null;
    if (!file) return;
    this.form.controls.file.setValue(file);
    this.form.controls.file.markAsTouched();
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  protected clearFile(): void {
    this.form.controls.file.setValue(null);
  }

  protected formatFileSize(bytes: number): string {
    return (bytes / (1024 * 1024)).toFixed(2);
  }

  protected submit(): void {
    if (this.submitting()) return;

    if (this.mode() === 'edit-title') {
      const titleControl = this.form.controls.title;
      if (titleControl.invalid) {
        titleControl.markAsTouched();
        return;
      }
      const material = this.editingMaterial();
      if (!material) return;
      this.submitted.emit({
        mode: 'edit-title',
        materialId: material.materialId,
        payload: { title: titleControl.value.trim() },
      });
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (!value.file) return;
    this.submitted.emit({
      mode: 'create',
      payload: {
        title: value.title.trim(),
        courseIds: (value.courses ?? []).map((c) => c.courseId),
        file: value.file,
      },
    });
  }
}
