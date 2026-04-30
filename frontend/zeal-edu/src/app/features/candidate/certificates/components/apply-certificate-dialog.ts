import {
  ChangeDetectionStrategy,
  Component,
  Signal,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { LowerCasePipe } from '@angular/common';
import { provideIcons } from '@ng-icons/core';
import {
  lucideCircleAlert,
  lucideCircleCheck,
  lucideCircleX,
} from '@ng-icons/lucide';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import { HlmSpinnerImports } from '@spartan-ng/helm/spinner';
import { CertificateService } from '../certificate.service';
import {
  ApplyForCertificatePayload,
  MyCertificateEligibility,
} from '../models/certificate-models';

interface DialogContext {
  batchId: string;
  batchLabel: string;
  courseName: string;
}

@Component({
  selector: 'app-apply-certificate-dialog',
  imports: [
    LowerCasePipe,
    HlmDialogImports,
    HlmButtonImports,
    HlmIconImports,
    HlmSpinnerImports,
  ],
  providers: [
    provideIcons({ lucideCircleCheck, lucideCircleX, lucideCircleAlert }),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <hlm-dialog #dlg>
      <hlm-dialog-content
        *hlmDialogPortal
        class="sm:max-w-md w-md"
        [showCloseButton]="true"
      >
        <div hlmDialogHeader>
          <h2 hlmDialogTitle>Apply for certificate</h2>
          @if (context(); as ctx) {
            <p class="text-muted-foreground text-sm">
              Submit a certificate application for {{ ctx.courseName }} ({{ ctx.batchLabel }}).
            </p>
          }
        </div>

        <div class="mt-2 flex flex-col gap-3">
          @if (eligibilityQuery.isPending()) {
            <div class="flex items-center gap-2 text-muted-foreground text-sm">
              <hlm-spinner /> Checking eligibility...
            </div>
          } @else if (eligibilityQuery.isError()) {
            <div class="rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
              Failed to check eligibility. Please try again.
            </div>
          } @else if (eligibilityQuery.data(); as e) {
            <ul class="flex flex-col gap-2 text-sm">
              <li class="flex items-start gap-2">
                <ng-icon
                  hlm
                  size="sm"
                  [name]="e.feesPaid ? 'lucideCircleCheck' : 'lucideCircleX'"
                  [class]="e.feesPaid ? 'text-emerald-600' : 'text-rose-600'"
                />
                <span>
                  Course fees: <strong>{{ e.feesPaid ? 'fully paid' : 'outstanding' }}</strong>
                </span>
              </li>
              <li class="flex items-start gap-2">
                <ng-icon
                  hlm
                  size="sm"
                  [name]="e.attendanceOk ? 'lucideCircleCheck' : 'lucideCircleX'"
                  [class]="e.attendanceOk ? 'text-emerald-600' : 'text-rose-600'"
                />
                <span>
                  Attendance: <strong>{{ e.attendancePercent }}%</strong>
                  <span class="text-muted-foreground"> (minimum 80%)</span>
                </span>
              </li>
              <li class="flex items-start gap-2">
                <ng-icon
                  hlm
                  size="sm"
                  [name]="e.examsPassed ? 'lucideCircleCheck' : 'lucideCircleX'"
                  [class]="e.examsPassed ? 'text-emerald-600' : 'text-rose-600'"
                />
                <span>
                  Exams: <strong>{{ e.examsPassed ? 'all passed' : 'incomplete or failed' }}</strong>
                </span>
              </li>
            </ul>

            @if (e.existingApplicationStatus) {
              <div class="rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800 flex items-start gap-2">
                <ng-icon hlm size="sm" name="lucideCircleAlert" class="mt-0.5" />
                <span>
                  An application is already
                  <strong>{{ e.existingApplicationStatus | lowercase }}</strong> for this batch.
                </span>
              </div>
            } @else if (!e.isEligible) {
              <div class="rounded-md border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
                {{ e.reason ?? 'You are not currently eligible for a certificate.' }}
              </div>
            }
          }
        </div>

        <div hlmDialogFooter>
          <button hlmBtn variant="outline" type="button" hlmDialogClose>Cancel</button>
          <button
            hlmBtn
            type="button"
            (click)="onSubmit()"
            [disabled]="!canSubmit() || submitting()"
          >
            @if (submitting()) {
              <hlm-spinner class="mr-2" />
              Submitting...
            } @else {
              Submit application
            }
          </button>
        </div>
      </hlm-dialog-content>
    </hlm-dialog>
  `,
})
export class ApplyCertificateDialog {
  private readonly _service = inject(CertificateService);

  readonly submitting = input<boolean>(false);
  readonly submitted = output<ApplyForCertificatePayload>();

  protected readonly dlg = viewChild<HlmDialog>('dlg');
  protected readonly context = signal<DialogContext | null>(null);

  private readonly _batchIdSignal: Signal<string | null> = computed(() => this.context()?.batchId ?? null);
  protected readonly eligibilityQuery = this._service.eligibilityQuery(this._batchIdSignal);

  protected readonly canSubmit = computed<boolean>(() => {
    const e: MyCertificateEligibility | undefined = this.eligibilityQuery.data();
    if (!e) return false;
    return e.isEligible && e.existingApplicationStatus === null;
  });

  open(ctx: DialogContext): void {
    this.context.set(ctx);
    this.dlg()?.open();
  }

  close(): void {
    this.dlg()?.close();
  }

  protected onSubmit(): void {
    if (!this.canSubmit() || this.submitting()) return;
    const ctx = this.context();
    if (!ctx) return;
    this.submitted.emit({ batchId: ctx.batchId });
  }
}
