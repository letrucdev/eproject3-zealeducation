import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCheckboxImports } from '@spartan-ng/helm/checkbox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { DigitsOnlyDirective } from '@shared/directives/digits-only.directive';
import { CourseListItem } from '@core/models/course-list-item';
import { CreateCoursePayload, UpdateCoursePayload } from '@core/services/courses.service';
import { HlmTextareaImports } from '@spartan-ng/helm/textarea';

export type CourseFormMode = 'create' | 'edit';

export interface CourseFormSubmitCreate {
  mode: 'create';
  payload: CreateCoursePayload;
}
export interface CourseFormSubmitUpdate {
  mode: 'edit';
  courseId: string;
  payload: UpdateCoursePayload;
}
export type CourseFormSubmit = CourseFormSubmitCreate | CourseFormSubmitUpdate;

@Component({
  selector: 'app-course-form-dialog',
  imports: [
    ReactiveFormsModule,
    HlmDialogImports,
    HlmFieldImports,
    HlmInputImports,
    HlmCheckboxImports,
    HlmButtonImports,
    HlmTextareaImports,
    DigitsOnlyDirective,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'course-form-dialog.html',
})
export class CourseFormDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<CourseFormSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly mode = signal<CourseFormMode>('create');
  readonly initial = signal<CourseListItem | null>(null);
  readonly isEdit = computed(() => this.mode() === 'edit');

  readonly form = this._fb.group({
    courseName: this._fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    description: this._fb.nonNullable.control('', [Validators.maxLength(2000)]),
    durationWeeks: this._fb.nonNullable.control<number | null>(null as unknown as number, [
      Validators.required,
      Validators.min(1),
      Validators.max(520),
    ]),
    baseFee: this._fb.nonNullable.control<number | null>(null as unknown as number, [
      Validators.required,
      Validators.min(0),
    ]),
    isActive: this._fb.nonNullable.control(true),
  });

  openCreate(): void {
    this.mode.set('create');
    this.initial.set(null);
    this.form.reset({
      courseName: '',
      description: '',
      durationWeeks: null as unknown as number,
      baseFee: null as unknown as number,
      isActive: true,
    });
    this.dlg()?.open();
  }

  openEdit(course: CourseListItem): void {
    this.mode.set('edit');
    this.initial.set(course);
    this.form.reset({
      courseName: course.courseName,
      description: course.description ?? '',
      durationWeeks: course.durationWeeks,
      baseFee: course.baseFee,
      isActive: course.isActive,
    });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    if (v.durationWeeks == null || v.baseFee == null) return;

    const payload: CreateCoursePayload = {
      courseName: v.courseName.trim(),
      description: v.description.trim() ? v.description.trim() : null,
      durationWeeks: Number(v.durationWeeks),
      baseFee: Number(v.baseFee),
      isActive: v.isActive,
    };

    if (this.isEdit()) {
      const current = this.initial();
      if (!current) return;
      this.submitted.emit({ mode: 'edit', courseId: current.courseId, payload });
    } else {
      this.submitted.emit({ mode: 'create', payload });
    }
  }
}
