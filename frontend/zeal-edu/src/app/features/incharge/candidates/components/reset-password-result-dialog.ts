import { ChangeDetectionStrategy, Component, signal, viewChild } from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideCheckCheck, lucideCopy } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { ResetCandidatePasswordResponse } from '../models/candidate-payload';

interface ResetPasswordResult extends ResetCandidatePasswordResponse {
  candidateName: string;
  candidateCode: string;
}

@Component({
  selector: 'app-reset-password-result-dialog',
  imports: [HlmDialogImports, HlmButtonImports, HlmIconImports],
  providers: [provideIcons({ lucideCopy, lucideCheckCheck })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'reset-password-result-dialog.html',
})
export class ResetPasswordResultDialog {
  protected readonly dlg = viewChild<HlmDialog>('dlg');

  readonly result = signal<ResetPasswordResult | null>(null);
  readonly copyState = signal<'idle' | 'copied'>('idle');

  open(result: ResetPasswordResult): void {
    this.result.set(result);
    this.copyState.set('idle');
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  async copyCredentials(): Promise<void> {
    const r = this.result();
    if (!r) return;
    const text = `Username: ${r.username}\nTemporary password: ${r.temporaryPassword}`;
    try {
      await navigator.clipboard.writeText(text);
      this.copyState.set('copied');
      setTimeout(() => this.copyState.set('idle'), 2000);
    } catch {
      this.copyState.set('idle');
    }
  }
}
