import { ChangeDetectionStrategy, Component, output, signal, viewChild } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';

export interface ConfirmDialogOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  destructive?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [HlmDialogImports, HlmButtonImports],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'confirm-dialog.html',
})
export class ConfirmDialog {
  readonly confirmed = output<void>();
  readonly cancelled = output<void>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  protected readonly title = signal('Are you sure?');
  protected readonly message = signal('');
  protected readonly confirmLabel = signal('Confirm');
  protected readonly cancelLabel = signal('Cancel');
  protected readonly destructive = signal(false);

  open(options: ConfirmDialogOptions): void {
    this.title.set(options.title);
    this.message.set(options.message);
    this.confirmLabel.set(options.confirmLabel ?? 'Confirm');
    this.cancelLabel.set(options.cancelLabel ?? 'Cancel');
    this.destructive.set(options.destructive ?? false);
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected onConfirm(): void {
    this.close();
    this.confirmed.emit();
  }

  protected onCancel(): void {
    this.close();
    this.cancelled.emit();
  }
}
