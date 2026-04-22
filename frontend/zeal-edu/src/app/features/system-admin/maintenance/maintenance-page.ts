import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-maintenance-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="flex flex-col gap-2">
      <h1 class="text-2xl font-semibold tracking-tight">Manage System Maintenance</h1>
      <p class="text-muted-foreground text-sm">Placeholder page.</p>
    </section>
  `,
})
export default class MaintenancePage {}
