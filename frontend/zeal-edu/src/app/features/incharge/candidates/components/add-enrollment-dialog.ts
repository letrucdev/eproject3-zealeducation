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
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { CourseListItem } from '@core/models/course-list-item';
import { CoursesService } from '@core/services/courses.service';
import { VndPipe } from '@shared/pipes/vnd-pipe';
import { AddEnrollmentPayload } from '../models/candidate-payload';

export interface AddEnrollmentSubmit {
  candidateId: string;
  payload: AddEnrollmentPayload;
}

interface AddEnrollmentTarget {
  candidateId: string;
  candidateCode: string;
  candidateName: string;
}

@Component({
  selector: 'app-add-enrollment-dialog',
  imports: [
    ReactiveFormsModule,
    VndPipe,
    HlmDialogImports,
    HlmFieldImports,
    HlmComboboxImports,
    HlmButtonImports,
    HlmSpinnerImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'add-enrollment-dialog.html',
})
export class AddEnrollmentDialog {
  private readonly _fb = inject(FormBuilder);
  private readonly _coursesService = inject(CoursesService);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<AddEnrollmentSubmit>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly target = signal<AddEnrollmentTarget | null>(null);

  readonly form = this._fb.group({
    course: this._fb.control<CourseListItem | null>(null, [Validators.required]),
  });

  protected readonly courseSearch = signal('');

  protected readonly courses = this._coursesService.listQuery(
    computed(() => ({
      page: 1,
      pageSize: 10,
      search: this.courseSearch(),
      isActive: true,
    })),
  );

  protected readonly selectedCourse = computed(() => this.form.controls.course.value);

  protected readonly courseItemToString = (course: CourseListItem | null): string =>
    course?.courseName ?? '';

  open(target: AddEnrollmentTarget): void {
    this.target.set(target);
    this.courseSearch.set('');
    this.form.reset({ course: null });
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  submit(): void {
    if (this.submitting()) return;
    if (this.form.invalid) return;
    const target = this.target();
    if (!target) return;

    const course = this.form.controls.course.value;
    if (!course) return;

    this.submitted.emit({
      candidateId: target.candidateId,
      payload: { courseId: course.courseId },
    });
  }
}
