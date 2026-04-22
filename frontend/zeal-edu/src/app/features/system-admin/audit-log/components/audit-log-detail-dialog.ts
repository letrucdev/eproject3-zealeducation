import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';
import { AuditAction } from '../../../../core/models/audit-action';
import { AuditLogDetail } from '../../../../core/models/audit-log-detail';

@Component({
  selector: 'app-audit-log-detail-dialog',
  imports: [
    DatePipe,
    HlmDialogImports,
    HlmButtonImports,
    HlmBadgeImports,
    HlmSkeletonImports,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'audit-log-detail-dialog.html',
})
export class AuditLogDetailDialog {
  readonly detail = input<AuditLogDetail | null | undefined>(null);
  readonly isLoading = input<boolean>(false);

  protected readonly dlg = viewChild<HlmDialog>('dlg');

  private readonly _isOpen = signal(false);

  readonly formattedOld = computed(() => this._format(this.detail()?.oldValue ?? null));
  readonly formattedNew = computed(() => this._format(this.detail()?.newValue ?? null));

  open(): void {
    this._isOpen.set(true);
    this.dlg()?.open();
  }

  close(): void {
    this._isOpen.set(false);
    this.dlg()?.close();
  }

  protected actionBadgeClass(action: AuditAction | undefined): string {
    switch (action) {
      case AuditAction.INSERT:
        return 'bg-emerald-100 text-emerald-800';
      case AuditAction.UPDATE:
        return 'bg-sky-100 text-sky-800';
      case AuditAction.DELETE:
        return 'bg-rose-100 text-rose-800';
      case AuditAction.OVERRIDE:
        return 'bg-amber-100 text-amber-900';
      default:
        return '';
    }
  }

  private _format(value: string | null): string {
    if (value === null || value === undefined) return '';
    try {
      return JSON.stringify(JSON.parse(value), null, 2);
    } catch {
      return value;
    }
  }
}
