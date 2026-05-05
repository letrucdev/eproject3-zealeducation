import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { CourseEnquiryListItem } from '@core/models/course-enquiry-list-item';
import { EnquiryFilterBar, EnquiryFilterValue } from './components/enquiry-filter-bar';
import { EnquiryFormDialog, EnquiryFormSubmit } from './components/enquiry-form-dialog';
import { EnquiryStatsCards } from './components/enquiry-stats-cards';
import { EnquiryTable } from './components/enquiry-table';
import { EnquiryDetailDialog, EnquiryNoteSubmit } from './components/enquiry-detail-dialog';
import { EnquiryConvertDialog, EnquiryConvertSubmit } from './components/enquiry-convert-dialog';
import { EnquirySource } from '@core/models/enquiry-source';
import { EnquiryStatus } from '@core/models/enquiry-status';
import { CourseEnquiriesService } from './course-enquiries.service';
import { EnquiryListQuery } from './models/course-enquiry-payload';
import { DataTableSortChange } from '@shared/components/data-table';

type DialogIntent = 'none' | 'edit' | 'view' | 'convert';

@Component({
  selector: 'app-course-enquiries-page',
  imports: [
    HlmCardImports,
    EnquiryStatsCards,
    EnquiryFilterBar,
    EnquiryTable,
    EnquiryFormDialog,
    EnquiryDetailDialog,
    EnquiryConvertDialog,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'course-enquiries-page.html',
})
export default class CourseEnquiriesPage {
  private readonly _service = inject(CourseEnquiriesService);

  protected readonly formDialog = viewChild.required<EnquiryFormDialog>('formDialog');
  protected readonly detailDialog = viewChild.required<EnquiryDetailDialog>('detailDialog');
  protected readonly convertDialog = viewChild.required<EnquiryConvertDialog>('convertDialog');

  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly statusFilter = signal<EnquiryStatus | ''>('');
  protected readonly sourceFilter = signal<EnquirySource | ''>('');
  protected readonly dueFollowUpOnly = signal(false);
  protected readonly sortBy = signal<string | null>(null);
  protected readonly sortDirection = signal<'asc' | 'desc'>('asc');

  protected readonly initialFilter: EnquiryFilterValue = {
    search: '',
    status: '',
    source: '',
    dueFollowUpOnly: false,
  };

  protected readonly focusedEnquiryId = signal<string | null>(null);
  private readonly _dialogIntent = signal<DialogIntent>('none');

  private readonly _listParams = computed<EnquiryListQuery>(() => ({
    page: this.page(),
    pageSize: this.pageSize(),
    search: this.search() || undefined,
    status: this.statusFilter() || undefined,
    source: this.sourceFilter() || undefined,
    dueFollowUpOnly: this.dueFollowUpOnly() || undefined,
    sortBy: this.sortBy() ?? undefined,
    sortDirection: this.sortDirection(),
  }));

  protected readonly listQuery = this._service.listQuery(this._listParams);
  protected readonly statsQuery = this._service.statisticsQuery();
  protected readonly detailQuery = this._service.detailQuery(this.focusedEnquiryId);
  protected readonly createMutation = this._service.createMutation();
  protected readonly updateMutation = this._service.updateMutation();
  protected readonly addNoteMutation = this._service.addNoteMutation();
  protected readonly convertMutation = this._service.convertMutation();

  protected readonly isSubmittingForm = computed(
    () => this.createMutation.isPending() || this.updateMutation.isPending(),
  );

  constructor() {
    effect(() => {
      const detail = this.detailQuery.data();
      const currentId = this.focusedEnquiryId();
      const intent = this._dialogIntent();
      if (!detail || currentId !== detail.enquiryId) return;

      untracked(() => {
        switch (intent) {
          case 'edit':
            this.formDialog().openEdit(detail);
            break;
          case 'view':
            this.detailDialog().open();
            break;
          case 'convert':
            this.convertDialog().open(detail);
            break;
        }
        this._dialogIntent.set('none');
      });
    });

    effect(() => {
      const error = this.detailQuery.error();
      if (!error) return;
      untracked(() => {
        this.focusedEnquiryId.set(null);
        this._dialogIntent.set('none');
      });
    });
  }

  onFilterChanged(value: EnquiryFilterValue): void {
    this.search.set(value.search);
    this.statusFilter.set(value.status);
    this.sourceFilter.set(value.source);
    this.dueFollowUpOnly.set(value.dueFollowUpOnly);
    this.page.set(1);
  }

  onPageChanged(page: number): void {
    this.page.set(page);
  }

  onPageSizeChanged(size: number): void {
    this.pageSize.set(size);
    this.page.set(1);
  }

  onSortChanged(change: DataTableSortChange): void {
    this.sortBy.set(change.sortBy);
    this.sortDirection.set(change.sortDirection);
    this.page.set(1);
  }

  onCreateClicked(): void {
    this.formDialog().openCreate();
  }

  onViewClicked(row: CourseEnquiryListItem): void {
    this._dialogIntent.set('view');
    this.focusedEnquiryId.set(row.enquiryId);
  }

  onEditClicked(row: CourseEnquiryListItem): void {
    this._dialogIntent.set('edit');
    this.focusedEnquiryId.set(row.enquiryId);
  }

  onConvertClicked(row: CourseEnquiryListItem): void {
    this._dialogIntent.set('convert');
    this.focusedEnquiryId.set(row.enquiryId);
  }

  onConvertFromDetail(detail: { enquiryId: string }): void {
    this.detailDialog().close();
    this._dialogIntent.set('convert');
    this.focusedEnquiryId.set(detail.enquiryId);
  }

  onFormSubmitted(event: EnquiryFormSubmit): void {
    if (event.mode === 'create') {
      this.createMutation.mutate(event.payload, {
        onSuccess: () => this.formDialog().close(),
        onError: (err) => this._handleFormConflict(err, event.payload.phone, event.payload.email),
      });
    } else {
      this.updateMutation.mutate(
        { enquiryId: event.enquiryId, payload: event.payload },
        {
          onSuccess: () => this.formDialog().close(),
          onError: (err) => this._handleFormConflict(err, event.payload.phone, event.payload.email),
        },
      );
    }
  }

  private _handleFormConflict(
    err: { status: number; error?: { message?: string } | null },
    phone: string,
    email: string | null,
  ): void {
    if (err.status !== 409) return;
    const message = err.error?.message?.toLowerCase() ?? '';
    if (message.includes('email') && email) {
      this.formDialog().markEmailTaken(email);
    } else {
      this.formDialog().markPhoneTaken(phone);
    }
  }

  onNoteSubmitted(event: EnquiryNoteSubmit): void {
    this.addNoteMutation.mutate(
      { enquiryId: event.enquiryId, payload: { content: event.content } },
      {
        onSuccess: () => {
          // re-open detail by retriggering focusedEnquiryId
          const id = event.enquiryId;
          this._dialogIntent.set('view');
          this.focusedEnquiryId.set(null);
          queueMicrotask(() => this.focusedEnquiryId.set(id));
        },
      },
    );
  }

  onConvertSubmitted(event: EnquiryConvertSubmit): void {
    this.convertMutation.mutate(
      { enquiryId: event.enquiryId, payload: event.payload },
      {
        onSuccess: (result) => this.convertDialog().showCredentials(result),
      },
    );
  }
}
