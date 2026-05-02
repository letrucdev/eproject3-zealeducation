import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, viewChild } from '@angular/core';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { CandidateStatus } from '@core/models/candidate-status';
import { CandidateDetail } from '@core/models/candidate-detail';
import { CandidateChangePasswordDialog } from './change-password-dialog';

@Component({
  selector: 'app-candidate-profile-info-card',
  imports: [
    DatePipe,
    HlmCardImports,
    HlmBadgeImports,
    HlmButtonImports,
    CandidateChangePasswordDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section hlmCard>
      <div hlmCardHeader class="flex flex-row flex-wrap items-start justify-between gap-3">
        <div class="flex flex-col gap-1">
          <h2 hlmCardTitle>{{ profile().fullName }}</h2>
          <p hlmCardDescription class="font-mono">{{ profile().candidateCode }}</p>
          <div class="mt-2 flex flex-wrap items-center gap-2">
            @switch (profile().status) {
              @case (statuses.Active) {
                <span hlmBadge class="bg-emerald-100 text-emerald-800">Active</span>
              }
              @case (statuses.OnBreak) {
                <span hlmBadge class="bg-sky-100 text-sky-800">On Break</span>
              }
              @case (statuses.Graduated) {
                <span hlmBadge class="bg-slate-100 text-slate-800">Graduated</span>
              }
              @case (statuses.Dropped) {
                <span hlmBadge class="bg-rose-100 text-rose-800">Dropped</span>
              }
            }
          </div>
        </div>
        <button hlmBtn variant="outline" size="sm" type="button" (click)="onChangePassword()">
          Change password
        </button>
      </div>
      <div hlmCardContent class="grid gap-4 md:grid-cols-2">
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Email</span>
          <span class="font-medium">{{ profile().email }}</span>
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Phone</span>
          <span class="font-medium">{{ profile().phone }}</span>
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Date of Birth</span>
          <span class="font-medium">{{ profile().dob | date: 'dd MMM yyyy' }}</span>
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Gender</span>
          <span class="font-medium">{{ profile().gender }}</span>
        </div>
        <div class="flex flex-col md:col-span-2">
          <span class="text-xs uppercase text-muted-foreground">Address</span>
          <span class="font-medium">{{ profile().address || '-' }}</span>
        </div>
        <div class="flex flex-col md:col-span-2">
          <span class="text-xs uppercase text-muted-foreground">Emergency Contact</span>
          <span class="font-medium">{{ profile().emergencyContact || '-' }}</span>
        </div>
        <div class="flex flex-col">
          <span class="text-xs uppercase text-muted-foreground">Registered At</span>
          <span class="font-medium">{{ profile().registeredAt | date: 'dd MMM yyyy' }}</span>
        </div>
      </div>
    </section>

    <app-candidate-change-password-dialog #passwordDialog />
  `,
})
export class CandidateProfileInfoCard {
  readonly profile = input.required<CandidateDetail>();

  protected readonly statuses = CandidateStatus;
  protected readonly passwordDialog = viewChild<CandidateChangePasswordDialog>('passwordDialog');

  protected onChangePassword(): void {
    this.passwordDialog()?.open();
  }
}
