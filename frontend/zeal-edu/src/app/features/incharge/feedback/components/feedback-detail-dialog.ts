import { ChangeDetectionStrategy, Component, computed, input, output, signal, viewChild } from '@angular/core';
import { DatePipe } from '@angular/common';
import { provideIcons } from '@ng-icons/core';
import { lucideCheckCheck, lucideStar, lucideUndo2 } from '@ng-icons/lucide';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { FEEDBACK_TYPE_BADGE_CLASSES } from '../models/feedback-labels';
import { FeedbackListItem, FeedbackType } from '../models/feedback-list-item';

@Component({
  selector: 'app-feedback-detail-dialog',
  imports: [
    DatePipe,
    HlmDialogImports,
    HlmButtonImports,
    HlmBadgeImports,
    HlmIconImports,
    HlmSpinnerImports,
  ],
  providers: [provideIcons({ lucideCheckCheck, lucideUndo2, lucideStar })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'feedback-detail-dialog.html',
})
export class FeedbackDetailDialog {
  readonly submitting = input<boolean>(false);
  readonly toggleProcessed = output<FeedbackListItem>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly current = signal<FeedbackListItem | null>(null);
  protected readonly typeBadgeClass = computed<string>(() => {
    const item = this.current();
    return item ? FEEDBACK_TYPE_BADGE_CLASSES[item.type] : '';
  });

  protected readonly typeLabel = (type: FeedbackType): string => type;

  open(item: FeedbackListItem): void {
    this.current.set(item);
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  onToggleClick(): void {
    if (this.submitting()) return;
    const item = this.current();
    if (!item) return;
    this.toggleProcessed.emit(item);
  }
}
