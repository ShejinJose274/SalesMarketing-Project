
import {
  Component,
  ChangeDetectorRef,
  OnDestroy,
  OnInit,
  ViewChild,
  inject
} from '@angular/core';

import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import { ActivatedRoute } from '@angular/router';

import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import {
  DetailRowService,
  GridComponent,
  GridModule,
  PageService,
  SortService
} from '@syncfusion/ej2-angular-grids';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import {
  DialogComponent,
  DialogModule
} from '@syncfusion/ej2-angular-popups';

import {
  Subject,
  Subscription,
  debounceTime,
  distinctUntilChanged
} from 'rxjs';

import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import {
  Salesman,
  SalesmanRequest
} from '../../models/api.models';

type ConfirmationType = 'save' | 'status' | 'delete';

@Component({
  selector: 'app-salesmen',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    DatePickerModule,
    DropDownListModule,
    GridModule,
    TextBoxModule,
    DialogModule
  ],
  providers: [DetailRowService,PageService,
  SortService],
  templateUrl: './salesmen.html',
  styleUrl: './salesmen.css'
})
export class Salesmen implements OnInit, OnDestroy {

  @ViewChild('salesmenGrid')
  salesmenGrid?: GridComponent;

  @ViewChild('confirmationDialog')
  confirmationDialog?: DialogComponent;

  readonly auth = inject(AuthService);

  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  salesmen: Salesman[] = [];
  gridData = { result: this.salesmen, count: 0 };
  totalSalesmen = 0;
  readonly pageSettings = {
  pageSize: 10,
  currentPage: 1,
  pageCount: 5,
  pageSizes: [5, 10, 20, 50]
};

  loading = true;
  saving = false;
  formOpen = false;

  editingId: number | null = null;
  inlineEditingId: number | null = null;

  search = '';
  filter = 'all';
  error = '';

  // Toast notifications
  toastMessage = '';
  toastType: 'success' | 'error' = 'success';
  toastVisible = false;

  private toastTimer?: ReturnType<typeof setTimeout>;
  private firstLoad = true;

  // Confirmation dialog
  confirmationOpen = false;
  confirmationTitle = '';
  confirmationMessage = '';
  confirmationButtonText = 'Confirm';
  confirmationType: ConfirmationType | null = null;
  confirmationDanger = false;

  pendingSalesman: Salesman | null = null;
  pendingStatus: number | null = null;
  confirming = false;

  // Preserve the browser scroll position
  private savedScrollY = 0;

  private readonly searchChanges = new Subject<string>();
  private searchSubscription?: Subscription;
  private loadSubscription?: Subscription;

  readonly salesmanForm = this.formBuilder.nonNullable.group({
    salesmancode: [
      '',
      [
        Validators.required,
        Validators.maxLength(20),
        Validators.pattern(/^[A-Za-z0-9]+$/)
      ]
    ],
    salesmanname: [
      '',
      [
        Validators.required,
        Validators.maxLength(100)
      ]
    ],
    mobileno: [
      '',
      [Validators.pattern(/^[0-9]{7,15}$/)]
    ],
    email: [
      '',
      [
        Validators.email,
        Validators.maxLength(100)
      ]
    ],
    joiningDate: this.formBuilder.control<Date | null>(null),
    status: [1, Validators.required]
  });

  // ==========================================
  // INITIALIZATION
  // ==========================================

  ngOnInit(): void {
    this.searchSubscription = this.searchChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => this.load(false, true));

    this.load();

    const isNew = this.route.snapshot.url.some(
      segment => segment.path === 'new'
    );

    if (isNew) {
      this.openCreate();
    }
  }

  // ==========================================
  // TOAST NOTIFICATIONS
  // ==========================================

  showToast(message: string, success = true): void {
    if (this.toastTimer) {
      clearTimeout(this.toastTimer);
    }

    this.toastMessage = message;
    this.toastType = success ? 'success' : 'error';
    this.toastVisible = true;
    this.cdr.detectChanges();

    this.toastTimer = setTimeout(() => {
      this.toastVisible = false;
      this.cdr.detectChanges();
    }, 4000);
  }

  dismissToast(): void {
    this.toastVisible = false;

    if (this.toastTimer) {
      clearTimeout(this.toastTimer);
    }

    this.cdr.detectChanges();
  }

  // ==========================================
  // SCROLL POSITION
  // ==========================================

  private rememberScroll(): void {
    this.savedScrollY = window.scrollY;
  }

  private restoreScroll(): void {
    requestAnimationFrame(() => {
      window.scrollTo({
        top: this.savedScrollY,
        behavior: 'instant'
      });
    });
  }

  // ==========================================
  // SEARCH / FILTER
  // ==========================================

  onSearchChange(value: string | null | undefined): void {
    this.search = value ?? '';
    // this.loadSubscription?.unsubscribe();
    this.searchChanges.next(this.search);
  }

  onFilterChange(): void {
  this.cancelInlineEdit();
  this.load(false, true);
}

  onGridDataStateChange(event: { skip?: number; take?: number }): void {
    const pageSize = event.take ?? this.pageSettings.pageSize;
    const page = Math.floor((event.skip ?? 0) / pageSize) + 1;

    this.load(false, false, page, pageSize);
  }

  // ==========================================
  // LOAD SALESMEN
  // ==========================================

  
load(
  preservePosition = false,
    resetPage = false,
    requestedPage?: number,
    requestedPageSize?: number
): void {
  this.loadSubscription?.unsubscribe();

  if (this.firstLoad) {
    this.loading = true;
  }

  const grid = this.salesmenGrid;

  const currentPage = resetPage
    ? 1
    : requestedPage ?? (grid?.pageSettings.currentPage ?? 1);
  const pageSize = requestedPageSize
    ?? grid?.pageSettings.pageSize
    ?? this.pageSettings.pageSize;

  const status =
    this.filter === 'all'
      ? undefined
      : Number(this.filter);

  this.loadSubscription = this.api.getSalesmen({
    search: this.search.trim(),
    status,
    page: currentPage,
    pageSize
  }).subscribe({
    next: response => {
      this.salesmen = [...response.items];
      this.totalSalesmen = response.totalCount;
      this.gridData = {
        result: this.salesmen,
        count: response.totalCount
      };

      this.loading = false;
      this.firstLoad = false;

      this.cdr.detectChanges();

      if (preservePosition) {
        this.restoreScroll();
      }

      this.cdr.detectChanges();
    },

    error: error => {
      this.loading = false;
      this.firstLoad = false;

      this.showToast(
        this.api.errorMessage(error),
        false
      );

      if (preservePosition) {
        this.restoreScroll();
      }

      this.cdr.detectChanges();
    }
  });
}


  // ==========================================
  // CREATE FORM
  // ==========================================

  openCreate(): void {
    if (this.saving || this.confirming) {
      return;
    }

    this.cancelInlineEdit();
    this.editingId = null;
    this.error = '';

    this.salesmanForm.reset({
      salesmancode: '',
      salesmanname: '',
      mobileno: '',
      email: '',
      joiningDate: null,
      status: 1
    });

    this.formOpen = true;
    this.cdr.detectChanges();
  }

  closeForm(): void {
    if (this.saving || this.confirming ||
        this.confirmationOpen) {
      return;
    }

    this.formOpen = false;
    this.error = '';
    this.cdr.detectChanges();
  }

  // ==========================================
  // INLINE EDIT
  // ==========================================

  private populateEditForm(person: Salesman): void {
    this.editingId = person.id;
    this.inlineEditingId = person.id;

    this.salesmanForm.reset({
      salesmancode: person.salesmancode,
      salesmanname: person.salesmanname,
      mobileno: person.mobileno ?? '',
      email: person.email ?? '',
      joiningDate: person.joiningdate
        ? new Date(`${person.joiningdate}T00:00:00`)
        : null,
      status: person.status
    });

    this.error = '';
    this.formOpen = false;
  }

  expandEditRow(person: Salesman): void {
    const grid = this.salesmenGrid;

    if (!grid || this.saving || this.confirming ||
        this.confirmationOpen) {
      return;
    }

    const records = grid.getCurrentViewRecords();

    const rowIndex = records.findIndex(
      (record: any) => record.id === person.id
    );

    if (rowIndex === -1) {
      this.showToast('Salesman row not found.', false);
      return;
    }

    if (this.inlineEditingId === person.id) {
      this.cancelInlineEdit();
      return;
    }

    if (this.inlineEditingId !== null) {
      const previousIndex = records.findIndex(
        (record: any) => record.id === this.inlineEditingId
      );

      if (previousIndex !== -1) {
        grid.detailRowModule.collapse(previousIndex);
      }
    }

    this.populateEditForm(person);
    this.cdr.detectChanges();

    try {
      grid.detailRowModule.expand(rowIndex);
      this.cdr.detectChanges();
    } catch (error) {
      console.error('Unable to expand edit row:', error);

      this.inlineEditingId = null;
      this.editingId = null;

      this.showToast('Unable to open the edit form.', false);
    }
  }

  cancelInlineEdit(): void {
    if (this.saving || this.confirming ||
        this.confirmationOpen) {
      return;
    }

    const grid = this.salesmenGrid;

    if (grid && this.inlineEditingId !== null) {
      const rowIndex = grid.getCurrentViewRecords()
        .findIndex(
          (record: any) => record.id === this.inlineEditingId
        );

      if (rowIndex !== -1) {
        grid.detailRowModule.collapse(rowIndex);
      }
    }

    this.inlineEditingId = null;
    this.editingId = null;
    this.error = '';
    this.cdr.detectChanges();
  }

  // ==========================================
  // VALIDATION AND REQUEST BUILDING
  // ==========================================

  private validateForm(): boolean {
    this.salesmanForm.markAllAsTouched();

    if (this.salesmanForm.invalid) {
      this.showToast(
        'Please correct the highlighted fields.',
        false
      );
      return false;
    }

    const joiningDate =
      this.salesmanForm.controls.joiningDate.value;

    if (joiningDate) {
      const selectedDate = new Date(
        joiningDate.getFullYear(),
        joiningDate.getMonth(),
        joiningDate.getDate()
      );

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      if (selectedDate > today) {
        this.showToast(
          'Joining date cannot be in the future.',
          false
        );
        return false;
      }
    }

    return true;
  }

  private buildRequest(): SalesmanRequest {
    const value = this.salesmanForm.getRawValue();

    const joiningdate = value.joiningDate
      ? [
          value.joiningDate.getFullYear(),
          String(value.joiningDate.getMonth() + 1).padStart(2, '0'),
          String(value.joiningDate.getDate()).padStart(2, '0')
        ].join('-')
      : null;

    return {
      salesmancode: value.salesmancode,
      salesmanname: value.salesmanname,
      mobileno: value.mobileno || null,
      email: value.email || null,
      joiningdate,
      status: value.status
    };
  }

  // ==========================================
  // OPEN CONFIRMATION
  // ==========================================

  openConfirmation(
    type: ConfirmationType,
    person: Salesman | null = null
  ): void {
    if (this.saving || this.confirming ||
        this.confirmationOpen) {
      return;
    }

    if (type === 'save' && !this.validateForm()) {
      return;
    }

    this.rememberScroll();

    this.confirmationType = type;
    this.pendingSalesman = person;
    this.pendingStatus = null;
    this.confirmationDanger = false;

    if (type === 'save') {
      const isCreate = this.editingId === null;

      this.confirmationTitle = isCreate
        ? 'Create Salesman'
        : 'Save Changes';

      this.confirmationMessage = isCreate
        ? 'Are you sure you want to create this salesman?'
        : 'Are you sure you want to save these changes?';

      this.confirmationButtonText = isCreate
        ? 'Create Salesman'
        : 'Save Changes';
    }

    if (type === 'status' && person) {
      const activating = person.status !== 1;

      this.pendingStatus = activating ? 1 : 2;

      this.confirmationTitle = activating
        ? 'Activate Salesman'
        : 'Deactivate Salesman';

      this.confirmationMessage =
        `Are you sure you want to ${
          activating ? 'activate' : 'deactivate'
        } ${person.salesmanname}?`;

      this.confirmationButtonText = activating
        ? 'Activate'
        : 'Deactivate';

      this.confirmationDanger = !activating;
    }

    if (type === 'delete' && person) {
      this.confirmationTitle = 'Delete Salesman';

      this.confirmationMessage =
        `Are you sure you want to delete ${person.salesmanname}? ` +
        'This action cannot be undone.';

      this.confirmationButtonText = 'Delete';
      this.confirmationDanger = true;
    }

    this.confirmationOpen = true;
    this.cdr.detectChanges();
  }

  closeConfirmation(): void {
    if (this.confirming) {
      return;
    }

    this.confirmationOpen = false;
    this.confirmationType = null;
    this.pendingSalesman = null;
    this.pendingStatus = null;
    this.confirmationDanger = false;
    this.cdr.detectChanges();
  }

  // ==========================================
  // CONFIRM ACTION
  // ==========================================

  confirmAction(): void {
    if (this.confirming || this.saving) {
      return;
    }

    const type = this.confirmationType;
    const person = this.pendingSalesman;
    const status = this.pendingStatus;

    if (type === 'save') {
      // Close the confirmation before calling save().
      this.confirmationOpen = false;
      this.confirmationType = null;
      this.pendingSalesman = null;
      this.pendingStatus = null;
      this.cdr.detectChanges();

      this.saveConfirmed();
      return;
    }

    if (!person) {
      return;
    }

    this.confirming = true;
    this.cdr.detectChanges();

    if (type === 'status' && status !== null) {
      this.api.changeSalesmanStatus(person.id, status)
        .subscribe({
          next: response => {
            this.finishConfirmedAction(response.message);
          },
          error: error => {
            this.confirmationFailed(error);
          }
        });

      return;
    }

    if (type === 'delete') {
      this.api.deleteSalesman(person.id).subscribe({
        next: response => {
          this.finishConfirmedAction(
            response.message,
            person.id
          );
        },
        error: error => {
          this.confirmationFailed(error);
        }
      });

      return;
    }

    this.confirming = false;
  }

  private finishConfirmedAction(
    message: string,
    deletedId: number | null = null
  ): void {
    this.confirming = false;
    this.closeConfirmation();

    if (deletedId !== null &&
        this.inlineEditingId === deletedId) {
      this.cancelInlineEdit();
    }

    this.showToast(message);
    this.load(true);
  }

  private confirmationFailed(error: unknown): void {
    this.confirming = false;
    this.closeConfirmation();

    this.showToast(
      this.api.errorMessage(error),
      false
    );

    this.restoreScroll();
  }

  // ==========================================
  // SAVE CONFIRMED
  // ==========================================

  private saveConfirmed(): void {
    if (this.saving || !this.validateForm()) {
      return;
    }

    const request = this.buildRequest();
    const editedId = this.editingId;

    this.saving = true;
    this.cdr.detectChanges();

    const success = () => {
      this.saving = false;

      if (editedId !== null) {
        this.cancelInlineEdit();
      }

      this.formOpen = false;

      this.showToast(
        editedId === null
          ? 'Salesman created successfully.'
          : 'Salesman updated successfully.'
      );

      this.load(true);
    };

    const failure = (error: unknown) => {
      this.saving = false;

      this.showToast(
        this.api.errorMessage(error),
        false
      );

      this.restoreScroll();
    };

    if (editedId === null) {
      this.api.createSalesman(request).subscribe({
        next: success,
        error: failure
      });
    } else {
      this.api.updateSalesman(editedId, request)
        .subscribe({
          next: success,
          error: failure
        });
    }
  }

  // ==========================================
  // ACTION BUTTON HANDLERS
  // ==========================================

  toggleStatus(person: Salesman): void {
    this.openConfirmation('status', person);
  }

  remove(person: Salesman): void {
    this.openConfirmation('delete', person);
  }

  // ==========================================
  // CLEANUP
  // ==========================================

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
    this.loadSubscription?.unsubscribe();

    if (this.toastTimer) {
      clearTimeout(this.toastTimer);
    }
  }
}
