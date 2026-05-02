import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideTriangleAlert } from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { CertificateApplicationListItem } from '../models/certificate-application';

@Component({
  selector: 'app-approve-certificate-dialog',
  imports: [HlmDialogImports, HlmButtonImports, HlmIconImports, HlmSpinnerImports],
  providers: [provideIcons({ lucideTriangleAlert })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-md w-md"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          <h2 hlmDialogTitle>Approve certificate application</h2>
          @if (current(); as row) {
            <p class="text-muted-foreground text-sm">
              You are about to approve and issue a certificate for {{ row.candidateName }}
              ({{ row.candidateCode }}) for the course <strong>{{ row.courseName }}</strong>.
            </p>
          }
        </div>

        @if (current(); as row) {
          <div class="mt-2 flex flex-col gap-2 rounded-md border bg-muted/40 p-3 text-sm">
            <div class="flex items-center justify-between">
              <span class="text-muted-foreground">Course fees</span>
              <span [class]="row.feesPaid ? 'text-emerald-700' : 'text-rose-700'">
                {{ row.feesPaid ? 'Fully paid' : 'Outstanding' }}
              </span>
            </div>
            <div class="flex items-center justify-between">
              <span class="text-muted-foreground">Attendance</span>
              <span [class]="row.attendanceOk ? 'text-emerald-700' : 'text-rose-700'">
                {{ row.attendancePercent }}% (min 80%)
              </span>
            </div>
            <div class="flex items-center justify-between">
              <span class="text-muted-foreground">Exams</span>
              <span [class]="row.examsPassed ? 'text-emerald-700' : 'text-rose-700'">
                {{ row.examsPassed ? 'All passed' : 'Incomplete or failed' }}
              </span>
            </div>
          </div>

          @if (!row.isCurrentlyEligible) {
            <div class="mt-3 flex items-start gap-2 rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
              <ng-icon hlm name="lucideTriangleAlert" size="sm" class="mt-0.5" />
              <span>This candidate is not currently eligible. Approval will be rejected.</span>
            </div>
          }
        }

        <div hlmDialogFooter>
          <button hlmBtn variant="outline" type="button" hlmDialogClose>Cancel</button>
          <button
            hlmBtn
            type="button"
            (click)="onConfirm()"
            [disabled]="!canConfirm() || submitting()"
          >
            @if (submitting()) {
              <hlm-spinner class="mr-2" />
              Approving...
            } @else {
              Approve and issue
            }
          </button>
        </div>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class ApproveCertificateDialog {
  readonly submitting = input<boolean>(false);
  readonly confirmed = output<CertificateApplicationListItem>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly current = signal<CertificateApplicationListItem | null>(null);

  protected readonly canConfirm = computed<boolean>(() => {
    const r = this.current();
    return !!r && r.status === 'Pending' && r.isCurrentlyEligible;
  });

  open(row: CertificateApplicationListItem): void {
    this.current.set(row);
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected onConfirm(): void {
    if (!this.canConfirm() || this.submitting()) return;
    const row = this.current();
    if (!row) return;
    this.confirmed.emit(row);
  }
}
