import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  Directive,
  computed,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { provideIcons } from '@ng-icons/core';
import { lucideAward, lucideDownload } from '@ng-icons/lucide';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmIconImports } from '@spartan-ng/helm/icon';
import {
  DataTable,
  DataTableCellContext,
  DataTableCellDef,
  DataTableColumn,
} from '@shared/components/data-table';
import { CertificateService } from './certificate.service';
import { ApplyCertificateDialog } from './components/apply-certificate-dialog';
import { MyCertificateListItem } from './models/certificate-models';

@Directive({
  selector: '[myCertificateCell]',
  providers: [{ provide: DataTableCellDef, useExisting: MyCertificateCellDef }],
})
export class MyCertificateCellDef extends DataTableCellDef<MyCertificateListItem> {
  override readonly appDataTableCell = input.required<string>({ alias: 'myCertificateCell' });

  static override ngTemplateContextGuard(
    _dir: MyCertificateCellDef,
    ctx: unknown,
  ): ctx is DataTableCellContext<MyCertificateListItem> {
    return true;
  }
}

@Component({
  selector: 'app-my-certificates-page',
  imports: [
    DatePipe,
    DataTable,
    MyCertificateCellDef,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    HlmIconImports,
    ApplyCertificateDialog,
  ],
  providers: [provideIcons({ lucideDownload, lucideAward })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-6">
      <header class="flex flex-col gap-2">
        <h1 class="text-2xl font-semibold tracking-tight">My Certificates</h1>
        <p class="text-muted-foreground max-w-2xl text-sm">
          Track certificate applications for completed courses. Once an incharge approves your
          application, you can download the PDF certificate from this page.
        </p>
      </header>

      <section hlmCard>
        <div hlmCardContent>
          <app-data-table
            [columns]="columns"
            [rows]="rows()"
            [isLoading]="listQuery.isPending()"
            [trackBy]="trackById"
            itemLabel="certificates"
            emptyMessage="You have no certificate applications yet."
          >
            <ng-template myCertificateCell="appliedAt" let-row>
              <span class="text-sm">{{ row.appliedAt | date: 'dd/MM/yyyy HH:mm' }}</span>
            </ng-template>

            <ng-template myCertificateCell="course" let-row>
              <div class="flex flex-col">
                <span class="font-medium">{{ row.courseName }}</span>
                @if (row.batchCode) {
                  <span class="text-muted-foreground text-xs font-mono">{{ row.batchCode }}</span>
                }
              </div>
            </ng-template>

            <ng-template myCertificateCell="status" let-row>
              @if (row.status === 'Approved') {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Approved</span>
              } @else {
                <span hlmBadge class="bg-amber-100 text-amber-800">Pending</span>
              }
            </ng-template>

            <ng-template myCertificateCell="certificateNumber" let-row>
              @if (row.certificateNumber) {
                <span class="font-mono text-xs">{{ row.certificateNumber }}</span>
              } @else {
                <span class="text-muted-foreground">—</span>
              }
            </ng-template>

            <ng-template myCertificateCell="approvedAt" let-row>
              @if (row.approvedAt) {
                <span class="text-sm">{{ row.approvedAt | date: 'dd/MM/yyyy' }}</span>
              } @else {
                <span class="text-muted-foreground">—</span>
              }
            </ng-template>

            <ng-template myCertificateCell="actions" let-row>
              <div class="flex items-center justify-end">
                @if (row.status === 'Approved') {
                  <button
                    hlmBtn
                    variant="outline"
                    size="sm"
                    type="button"
                    [disabled]="downloadingId() === row.applicationId"
                    (click)="onDownload(row)"
                  >
                    <ng-icon hlm name="lucideDownload" size="sm" class="mr-1" />
                    Download
                  </button>
                } @else {
                  <span class="text-muted-foreground text-xs">Awaiting approval</span>
                }
              </div>
            </ng-template>
          </app-data-table>
        </div>
      </section>

      <app-apply-certificate-dialog
        #applyDialog
        [submitting]="applyMutation.isPending()"
        (submitted)="onApplySubmitted($event)"
      />
    </section>
  `,
})
export default class MyCertificatesPage {
  private readonly _service = inject(CertificateService);

  protected readonly listQuery = this._service.myCertificatesQuery();
  protected readonly applyMutation = this._service.applyMutation();

  protected readonly applyDialog = viewChild<ApplyCertificateDialog>('applyDialog');
  protected readonly downloadingId = signal<string | null>(null);

  protected readonly columns: DataTableColumn<MyCertificateListItem>[] = [
    { key: 'appliedAt', header: 'Applied at' },
    { key: 'course', header: 'Course' },
    { key: 'status', header: 'Status' },
    { key: 'certificateNumber', header: 'Certificate No.' },
    { key: 'approvedAt', header: 'Issued on' },
    { key: 'actions', header: 'Actions', align: 'right' },
  ];

  protected readonly trackById = (row: MyCertificateListItem): string => row.applicationId;

  protected readonly rows = computed<MyCertificateListItem[]>(() => this.listQuery.data() ?? []);

  protected onApplySubmitted(payload: { batchId: string }): void {
    this.applyMutation.mutate(payload, {
      onSuccess: () => {
        toast.success('Certificate application submitted.');
        this.applyDialog()?.close();
      },
    });
  }

  protected async onDownload(row: MyCertificateListItem): Promise<void> {
    if (row.status !== 'Approved') return;
    if (this.downloadingId() === row.applicationId) return;
    this.downloadingId.set(row.applicationId);
    try {
      const { blob, fileName } = await this._service.downloadCertificateFile(row.applicationId);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch {
      toast.error('Failed to download certificate.');
    } finally {
      this.downloadingId.set(null);
    }
  }
}
