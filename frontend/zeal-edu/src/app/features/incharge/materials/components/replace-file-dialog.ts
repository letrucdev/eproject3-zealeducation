import {
  ChangeDetectionStrategy,
  Component,
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
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { ReplaceMaterialFilePayload, StudyMaterialListItem } from '../models/material-payload';

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

export interface ReplaceFileSubmit {
  materialId: string;
  payload: ReplaceMaterialFilePayload;
}

@Component({
  selector: 'app-replace-file-dialog',
  imports: [
    ReactiveFormsModule,
    HlmButtonImports,
    HlmDialogImports,
    HlmFieldImports,
    HlmSpinnerImports,
    NgIcon,
  ],
  providers: [provideIcons({ lucideUpload, lucideX })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'replace-file-dialog.html',
})
export class ReplaceFileDialog {
  private readonly _fb = inject(FormBuilder);

  readonly submitted = output<ReplaceFileSubmit>();
  readonly submitting = input(false);

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly target = signal<StudyMaterialListItem | null>(null);

  protected readonly form = this._fb.group({
    file: this._fb.control<File | null>(null, [
      Validators.required,
      fileSizeValidator,
      fileExtensionValidator,
    ]),
  });

  open(material: StudyMaterialListItem): void {
    this.target.set(material);
    this.form.reset({ file: null });
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const file = this.form.controls.file.value;
    const target = this.target();
    if (!file || !target) return;
    this.submitted.emit({ materialId: target.materialId, payload: { file } });
  }
}
