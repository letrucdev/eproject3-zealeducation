import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideDownload,
  lucideEye,
  lucideEyeOff,
  lucideFile,
  lucideFileSpreadsheet,
  lucideFileText,
  lucideFileType,
  lucideFileVideo,
  lucideImage,
  lucidePencil,
  lucideRefreshCcw,
  lucideTrash2,
} from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';

export interface MaterialCardData {
  materialId: string;
  title: string;
  fileName: string;
  fileType: string;
  fileSizeMb: number;
  isActive: boolean;
  uploadedAt: string;
}

export type MaterialCardActionKind =
  | 'download'
  | 'edit-title'
  | 'replace-file'
  | 'toggle-active'
  | 'delete';

export interface MaterialCardAction<T extends MaterialCardData = MaterialCardData> {
  kind: MaterialCardActionKind;
  material: T;
}

@Component({
  selector: 'app-material-card-item',
  imports: [HlmButtonImports, NgIcon],
  providers: [
    provideIcons({
      lucideDownload,
      lucideEye,
      lucideEyeOff,
      lucideFile,
      lucideFileSpreadsheet,
      lucideFileText,
      lucideFileType,
      lucideFileVideo,
      lucideImage,
      lucidePencil,
      lucideRefreshCcw,
      lucideTrash2,
    }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'material-card-item.html',
})
export class MaterialCardItem {
  readonly material = input.required<MaterialCardData>();
  readonly readOnly = input<boolean>(false);
  readonly action = output<MaterialCardAction>();

  protected readonly fileIconName = computed(() => mapFileIcon(this.material().fileType));
  protected readonly uploadedAtLabel = computed(() => formatDate(this.material().uploadedAt));

  protected onAction(kind: MaterialCardActionKind, event: Event): void {
    event.stopPropagation();
    if (this.readOnly() && kind !== 'download') return;
    this.action.emit({ kind, material: this.material() });
  }

  protected onCardClick(): void {
    this.action.emit({ kind: 'download', material: this.material() });
  }
}

function mapFileIcon(fileType: string): string {
  switch (fileType.toLowerCase()) {
    case 'pdf':
      return 'lucideFileText';
    case 'mp4':
    case 'mov':
      return 'lucideFileVideo';
    case 'jpg':
    case 'jpeg':
    case 'png':
      return 'lucideImage';
    case 'doc':
    case 'docx':
      return 'lucideFileType';
    case 'xls':
    case 'xlsx':
      return 'lucideFileSpreadsheet';
    default:
      return 'lucideFile';
  }
}

function formatDate(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return '';
  return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' });
}
