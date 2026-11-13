
// import { CommonModule } from '@angular/common';
// import {
//   Component,
//   ChangeDetectorRef,
//   OnInit,
//   OnDestroy,
//   ViewChild,
//   inject
// } from '@angular/core';
// import { FormsModule } from '@angular/forms';

// import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
// import {
//   GridModule,
//   GridComponent,
//   PageSettingsModel,
//     PageService,
//   SortService
// } from '@syncfusion/ej2-angular-grids';
// import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
// import {
//   TextBoxModule,
//   NumericTextBoxModule
// } from '@syncfusion/ej2-angular-inputs';
// import {
//   DialogModule,
//   DialogComponent
// } from '@syncfusion/ej2-angular-popups';

// import {
//   Subject,
//   Subscription,
//   debounceTime,
//   distinctUntilChanged
// } from 'rxjs';

// import { ApiService } from '../../services/api.service';

// import {
//   MarketingTeam,
//   MarketingTeamListItem,
//   MarketingTeamRequest,
//   Salesman,
//   SALESMAN_ROLES
// } from '../../models/api.models';

// type ConfirmAction =
//   'save' | 'delete' | 'status' | 'removeMember';

// @Component({
//   selector: 'app-marketing-teams',
//   standalone: true,
//   imports: [
//     CommonModule,
//     FormsModule,
//     ButtonModule,
//     GridModule,
//     DropDownListModule,
//     TextBoxModule,
//     NumericTextBoxModule,
//     DialogModule
//   ],
//    providers: [
//     PageService,
//     SortService
//   ],
//   templateUrl: './marketing-teams.html',
//   styleUrl: './marketing-teams.css'
// })
// export class MarketingTeams implements OnInit, OnDestroy {
//   private readonly api = inject(ApiService);
//   private readonly cdr = inject(ChangeDetectorRef);

//   @ViewChild('teamGrid')
//   teamGrid?: GridComponent;

//   @ViewChild('viewDialog')
//   viewDialog?: DialogComponent;

//   @ViewChild('editDialog')
//   editDialog?: DialogComponent;

//   teams: MarketingTeamListItem[] = [];
//   availableSalesmen: Salesman[] = [];

//   readonly roles = SALESMAN_ROLES;

//   readonly roleOptions = Object.entries(SALESMAN_ROLES)
//     .filter(([key]) => Number(key) !== 1)
//     .map(([key, value]) => ({
//       key: Number(key),
//       value
//     }));

//   readonly statusOptions = [
//     { text: 'Active', value: 1 },
//     { text: 'Inactive', value: 2 }
//   ];

//   readonly pageSettings: PageSettingsModel = {
//     pageSize: 10,
//     pageSizes: [10, 20, 50]
//   };

//   search = '';
//   selectedStatus: number | null = null;
//   readonly filterOptions = [{ text: 'All teams', value: 0 }, { text: 'Active', value: 1 }, { text: 'Inactive', value: 2 }];
//   totalTeams = 0;
//   activeTeams = 0;
//   inactiveTeams = 0;
//   readonly maxMembers = 5;
//   get memberLimitReached(): boolean { return this.form.members.length >= this.maxMembers; }
//   get membershipLocked(): boolean { return this.editingId !== null && this.form.status === 2; }
//   loading = false;
//   saving = false;
//   processing = false;

//   // Team details modal
//   viewOpen = false;
//   viewLoading = false;
//   selectedTeam: MarketingTeam | null = null;

//   // Create/edit modal
//   formOpen = false;
//   editingId: number | null = null;
//   form: MarketingTeamRequest = this.emptyForm();
//   formError = '';

//   memberNames: Record<number, string> = {};
//   memberCodes: Record<number, string> = {};

//   memberSalesmanId: number | null = null;
//   memberRole = 3;
//   memberTarget = 0;

//   // Confirmation modal
//   confirmOpen = false;
//   confirmTitle = '';
//   confirmMessage = '';
//   confirmButton = 'Confirm';
//   confirmDanger = false;

//   pendingAction: ConfirmAction | null = null;
//   pendingTeam: MarketingTeamListItem | MarketingTeam | null = null;
//   pendingMemberId: number | null = null;

//   // Toast
//   toastVisible = false;
//   toastMessage = '';
//   toastSuccess = true;

//   private toastTimer?: ReturnType<typeof setTimeout>;
//   private readonly searchInput = new Subject<string>();
//   private searchSub?: Subscription;
//   private listSub?: Subscription;
//   private detailSub?: Subscription;

//   ngOnInit(): void {
//   this.searchSub = this.searchInput.pipe(
//     debounceTime(300),
//     distinctUntilChanged()
//   ).subscribe(() => {
//     console.log('Debounced search triggered');
//     this.load();
//   });

//   this.load();
//   this.loadSummary();
// }

//   ngOnDestroy(): void {
//     this.searchSub?.unsubscribe();
//     this.listSub?.unsubscribe();
//     this.detailSub?.unsubscribe();

//     if (this.toastTimer) {
//       clearTimeout(this.toastTimer);
//     }
//   }

//   get activeTeamCount(): number {
//     return this.activeTeams;
//   }

//   get inactiveTeamCount(): number {
//     return this.inactiveTeams;
//   }

//   get totalTarget(): number {
//     return this.form.members.reduce(
//       (sum, member) => sum + Number(member.targetamount || 0),
//       0
//     );
//   }

//   get viewTotalTarget(): number {
//     return this.selectedTeam?.members.reduce(
//       (sum, member) => sum + Number(member.targetamount || 0),
//       0
//     ) ?? 0;
//   }

//   // Available unassigned salesmen, plus current team members when editing.
//   get leaderOptions() {
//     const options = new Map<number, { id: number; name: string }>();
//     for (const person of this.availableSalesmen) {
//       if (person.status === 1) options.set(person.id, { id: person.id, name: `${person.salesmanname} (${person.salesmancode})` });
//     }
//     for (const member of this.form.members) {
//       if (!options.has(member.salesmanid)) {
//         options.set(member.salesmanid, { id: member.salesmanid, name: this.memberName(member.salesmanid) });
//       }
//     }
//     return [...options.values()];
//   }

//   get selectableSalesmen(): Salesman[] {
//     return this.availableSalesmen.filter(person =>
//       !this.form.members.some(
//         member => member.salesmanid === person.id
//       )
//     );
//   }

//   private emptyForm(): MarketingTeamRequest {
//     return {
//       teamcode: '',
//       teamname: '',
//       description: null,
//       teamleaderid: 0,
//       status: 1,
//       members: []
//     };
//   }

//   // ==============================
//   // NOTIFICATIONS
//   // ==============================

//   showToast(message: string, success = true): void {
//     if (this.toastTimer) {
//       clearTimeout(this.toastTimer);
//     }

//     this.toastMessage = message;
//     this.toastSuccess = success;
//     this.toastVisible = true;
//     this.cdr.detectChanges();

//     this.toastTimer = setTimeout(() => {
//       this.toastVisible = false;
//       this.cdr.detectChanges();
//     }, 4000);
//   }

//   dismissToast(): void {
//     this.toastVisible = false;
//     this.cdr.detectChanges();
//   }

//   // ==============================
//   // SEARCH AND GRID
//   // ==============================


// onSearch(value: string | null | undefined): void {
//   console.log('SEARCH EVENT:', value);

//   this.search = value ?? '';


//   this.searchInput.next(this.search);
// }

// onStatusChange(value: number | string | null | undefined): void {
  

//   const status = Number(value);

//   this.selectedStatus =
//     status === 1 || status === 2 ? status : null;

//   console.log('STATUS STATE:', this.selectedStatus);

//   this.load();
// }


//   private loadSummary(): void {
//     // These totals must not depend on the search box or status filter.
//     this.api.getTeams({ pageSize: 1 }).subscribe({
//       next: response => { this.totalTeams = response.totalCount; this.cdr.detectChanges(); },
//       error: error => this.showToast(this.api.errorMessage(error), false)
//     });
//     this.api.getTeams({ status: 1, pageSize: 1 }).subscribe({
//       next: response => { this.activeTeams = response.totalCount; this.cdr.detectChanges(); },
//       error: error => this.showToast(this.api.errorMessage(error), false)
//     });
//     this.api.getTeams({ status: 2, pageSize: 1 }).subscribe({
//       next: response => { this.inactiveTeams = response.totalCount; this.cdr.detectChanges(); },
//       error: error => this.showToast(this.api.errorMessage(error), false)
//     });
//   }

  
// load(): void {
//   console.log('LOAD CALLED:', {
//     search: this.search,
//     status: this.selectedStatus
//   });

//   // Cancel the previous API request.
//   this.listSub?.unsubscribe();

//   this.loading = true;

//   const query = {
//     search: this.search.trim(),
//     status: this.selectedStatus ?? undefined,
//     page: 1,
//     pageSize: 100
//   };

//   console.log('API REQUEST PARAMETERS:', query);

//   this.listSub = this.api.getTeams(query).subscribe({
//     next: response => {
//   console.log('API ITEMS:', response.items);

//   this.teams = [...response.items];
//   this.loading = false;

//   // Update Angular bindings first.
//   this.cdr.detectChanges();

//   // Return to the first page after filtering.
//   if (this.teamGrid) {
//     this.teamGrid.goToPage(1);
//   }

//   this.cdr.detectChanges();
// },

//     error: error => {
//       console.error('MARKETING TEAM API ERROR:', error);

//       this.loading = false;
//       this.teams = [];

//       this.showToast(
//         this.api.errorMessage(error),
//         false
//       );

//       this.cdr.detectChanges();
//     }
//   });
// }

//   // ==============================
//   // VIEW TEAM
//   // ==============================

//   openView(team: MarketingTeamListItem): void {
//     this.detailSub?.unsubscribe();

//     this.viewOpen = true;
//     this.viewLoading = true;
//     this.selectedTeam = null;
//     this.cdr.detectChanges();

//     this.detailSub = this.api.getTeam(team.id).subscribe({
//       next: detail => {
//         this.selectedTeam = detail;
//         this.viewLoading = false;
//         this.cdr.detectChanges();
//       },
//       error: error => {
//         this.viewLoading = false;
//         this.viewOpen = false;
//         this.showToast(this.api.errorMessage(error), false);
//       }
//     });
//   }

//   closeView(): void {
//     if (this.processing || this.confirmOpen) return;

//     this.viewOpen = false;
//     this.detailSub?.unsubscribe();
//     this.cdr.detectChanges();
//   }

//   roleName(role: number): string {
//     return this.roles[role] ?? `Role ${role}`;
//   }

//   // ==============================
//   // CREATE TEAM
//   // ==============================

//   openCreate(): void {
//     this.editingId = null;
//     this.form = this.emptyForm();

//     this.memberNames = {};
//     this.memberCodes = {};

//     this.memberSalesmanId = null;
//     this.memberRole = 3;
//     this.memberTarget = 0;

//     this.formError = '';
//     this.viewOpen = false;
//     this.formOpen = true;

//     this.loadAvailableSalesmen();
//     this.cdr.detectChanges();
//   }

//   // ==============================
//   // EDIT TEAM
//   // ==============================

//   openEdit(team: MarketingTeamListItem | MarketingTeam): void {
//     this.api.getTeam(team.id).subscribe({
//       next: detail => {
//         this.editingId = detail.id;

//         this.memberNames = {};
//         this.memberCodes = {};

//         detail.members.forEach(member => {
//           this.memberNames[member.salesmanid] =
//             member.salesmanname;
//         });

//         this.form = {
//           teamcode: detail.teamcode,
//           teamname: detail.teamname,
//           description: detail.description,
//           teamleaderid: detail.teamleaderid,
//           status: detail.status,
//           members: detail.members.map(member => ({
//             salesmanid: member.salesmanid,
//             role: member.role,
//             targetamount: member.targetamount,
//             status: member.status
//           }))
//         };

//         this.memberSalesmanId = null;
//         this.memberRole = 3;
//         this.memberTarget = 0;

//         this.formError = '';
//         this.viewOpen = false;
//         this.formOpen = true;

//         this.loadAvailableSalesmen();
//         this.cdr.detectChanges();
//       },
//       error: error => {
//         this.showToast(this.api.errorMessage(error), false);
//       }
//     });
//   }

//   closeForm(): void {
//     if (this.saving || this.confirmOpen) return;

//     this.formOpen = false;
//     this.editDialog?.hide();
//     this.formError = '';
//     this.cdr.detectChanges();
//   }

//   private loadAvailableSalesmen(): void {
//     this.api.getAvailableSalesmen().subscribe({
//       next: salesmen => {
//         this.availableSalesmen = salesmen;
//         this.cdr.detectChanges();
//       },
//       error: error => {
//         this.showToast(this.api.errorMessage(error), false);
//       }
//     });
//   }

//   memberName(id: number): string {
//     return this.memberNames[id] ??
//       this.availableSalesmen.find(x => x.id === id)
//         ?.salesmanname ??
//       `Salesman #${id}`;
//   }

//   memberCode(id: number): string {
//     return this.memberCodes[id] ??
//       this.availableSalesmen.find(x => x.id === id)
//         ?.salesmancode ??
//       `#${id}`;
//   }



  
// deleteEditingTeam(): void {
//   if (this.editingId === null) {
//     return;
//   }

//   // Find the complete team object from the loaded list.
//   const team = this.teams.find(
//     x => x.id === this.editingId
//   );

//   if (!team) {
//     this.showToast('Marketing team not found.', false);
//     return;
//   }

//   // Open the existing delete confirmation modal.
//   this.requestDelete(team);
// }


//   // ==============================
//   // MEMBER MANAGEMENT
//   // ==============================

//   addMember(): void {
//     if (this.membershipLocked) { this.formError = 'This team is inactive. Activate it before adding members.'; this.cdr.detectChanges(); return; }
//     if (this.memberLimitReached) { this.formError = 'A team can contain a maximum of 5 members, including the leader.'; this.cdr.detectChanges(); return; }
//     const id = Number(this.memberSalesmanId);

//     const person = this.selectableSalesmen.find(
//       x => x.id === id
//     );

//     if (!person) {
//       this.showToast('Select an available salesman.', false);
//       return;
//     }

//     const isFirst = this.form.members.length === 0;

//     this.memberNames[id] = person.salesmanname;
//     this.memberCodes[id] = person.salesmancode;

//     this.form.members = [
//       ...this.form.members,
//       {
//         salesmanid: id,
//         role: isFirst ? 1 : Number(this.memberRole),
//         targetamount: Number(this.memberTarget) || 0,
//         status: 1
//       }
//     ];

//     if (isFirst) {
//       this.form.teamleaderid = id;
//     }

//     this.memberSalesmanId = null;
//     this.memberRole = 3;
//     this.memberTarget = 0;

//     this.cdr.detectChanges();
//   }

//   setLeader(id: number): void {
//     const leaderId = Number(id);
//     if (!leaderId) return;
//     if (this.membershipLocked) {
//       this.formError = 'Activate this team before changing its leader.';
//       this.cdr.detectChanges();
//       return;
//     }
//     const alreadyMember = this.form.members.some(m => m.salesmanid === leaderId);
//     if (!alreadyMember) {
//       if (this.memberLimitReached) {
//         this.formError = 'Maximum 5 members including the leader. Remove a member first.';
//         this.cdr.detectChanges();
//         return;
//       }
//       const person = this.availableSalesmen.find(p => p.id === leaderId);
//       if (!person) {
//         this.formError = 'Select an available, active salesman.';
//         this.cdr.detectChanges();
//         return;
//       }
//       this.memberNames[leaderId] = person.salesmanname;
//       this.memberCodes[leaderId] = person.salesmancode;
//       this.form.members = [...this.form.members, {
//         salesmanid: leaderId, role: 1, targetamount: 0, status: 1
//       }];
//     }
//     this.form.teamleaderid = leaderId;
//     this.form.members = this.form.members.map(member => ({
//       ...member,
//       role: member.salesmanid === leaderId ? 1 : (member.role === 1 ? 2 : member.role)
//     }));
//     this.formError = '';
//     this.cdr.detectChanges();
//   }

//   requestRemoveMember(id: number): void {
//     if (this.membershipLocked) { this.formError = 'Activate this team before changing its members.'; this.cdr.detectChanges(); return; }
//     if (this.form.teamleaderid === id) {
//       this.showToast(
//         'Select another team leader before removing this member.',
//         false
//       );
//       return;
//     }

//     this.pendingMemberId = id;
//     this.pendingAction = 'removeMember';
//     this.confirmTitle = 'Remove Team Member';
//     this.confirmMessage =
//       `Remove ${this.memberName(id)} from the team? ` +
//       'This change will take effect after saving.';
//     this.confirmButton = 'Remove Member';
//     this.confirmDanger = true;
//     this.confirmOpen = true;
//     this.cdr.detectChanges();
//   }

//   private removeMemberConfirmed(id: number): void {
//     this.form.members = this.form.members.filter(
//       member => member.salesmanid !== id
//     );

//     this.showToast(
//       'Member removed from the form. Save to apply changes.'
//     );
//     this.cdr.detectChanges();
//   }

//   // ==============================
//   // SAVE
//   // ==============================

//   requestSave(): void {
//     this.formError = '';

//     if (!this.form.teamcode.trim()) {
//       this.formError = 'Team code is required.';
//     } else if (!this.form.teamname.trim()) {
//       this.formError = 'Team name is required.';
//     } else if (this.form.members.length > this.maxMembers) {
//       this.formError = 'A team can contain a maximum of 5 members.';
//     } else if (!this.form.members.length) {
//       this.formError = 'Add at least one team member.';
//     } else if (!this.form.teamleaderid ||
//       !this.form.members.some(
//         m => m.salesmanid === Number(this.form.teamleaderid)
//       )) {
//       this.formError = 'Select a valid team leader.';
//     } else if (this.form.members.some(
//       m => Number(m.targetamount) < 0
//     )) {
//       this.formError = 'Target amount cannot be negative.';
//     }

//     if (this.formError) {
//       this.showToast(this.formError, false);
//       return;
//     }

//     this.pendingAction = 'save';
//     this.confirmTitle = this.editingId === null
//       ? 'Create Marketing Team'
//       : 'Update Marketing Team';
//     this.confirmMessage =
//       'Are you sure you want to save these team details?';
//     this.confirmButton = this.editingId === null
//       ? 'Create Team'
//       : 'Update Team';
//     this.confirmDanger = false;
//     this.confirmOpen = true;
//     this.cdr.detectChanges();
//   }

//   private saveConfirmed(): void {
//     if (this.saving) return;

//     this.setLeader(this.form.teamleaderid);
//     this.saving = true;

//     const request: MarketingTeamRequest = {
//       ...this.form,
//       members: this.form.members.map(m => ({ ...m }))
//     };

//     const id = this.editingId;

//     const success = () => {
//       this.saving = false;
//       this.formOpen = false;
//       this.editDialog?.hide();
//       this.editingId = null;
//       this.formError = '';
//       this.cdr.detectChanges();

//       this.showToast(
//         id === null
//           ? 'Marketing team created successfully.'
//           : 'Marketing team updated successfully.'
//       );

//       this.load();
//       this.loadSummary();
//     };

//     const failure = (error: unknown) => {
//       this.saving = false;
//       this.formError = this.api.errorMessage(error);
//       this.showToast(this.formError, false);
//     };

//     if (id === null) {
//       this.api.createTeam(request).subscribe({
//         next: success,
//         error: failure
//       });
//     } else {
//       this.api.updateTeam(id, request).subscribe({
//         next: success,
//         error: failure
//       });
//     }
//   }

//   // ==============================
//   // STATUS
//   // ==============================

//   requestStatus(team: MarketingTeamListItem | MarketingTeam): void {
//     const activate = team.status !== 1;

//     this.pendingTeam = team;
//     this.pendingAction = 'status';

//     this.confirmTitle = activate
//       ? 'Activate Marketing Team'
//       : 'Deactivate Marketing Team';

//     this.confirmMessage =
//       `Are you sure you want to ${activate ? 'activate' : 'deactivate'} ` +
//       `${team.teamname}?`;

//     this.confirmButton = activate ? 'Activate' : 'Deactivate';
//     this.confirmDanger = !activate;
//     this.confirmOpen = true;
//     this.cdr.detectChanges();
//   }

//   // ==============================
//   // DELETE
//   // ==============================

  
// requestDelete(team: MarketingTeamListItem | MarketingTeam): void {

//   // Active teams cannot be deleted.
//   if (team.status === 1) {
//     this.showToast('Deactivate team first to delete', false);
//     return;
//   }

//   // Only inactive teams can be deleted.
//   if (team.status !== 2) {
//     this.showToast('Only inactive teams can be deleted.', false);
//     return;
//   }

//   // Open the existing confirmation dialog.
//   this.pendingTeam = team;
//   this.pendingAction = 'delete';

//   this.confirmTitle = 'Delete Marketing Team';

//   this.confirmMessage =
//     `Are you sure you want to delete ${team.teamname}? ` +
//     'The team will be marked as deleted and its history preserved.';

//   this.confirmButton = 'Delete Team';
//   this.confirmDanger = true;
//   this.confirmOpen = true;

//   this.cdr.detectChanges();
// }




//   // ==============================
//   // CONFIRMATION
//   // ==============================

//   closeConfirmation(): void {
//     if (this.processing || this.saving) return;

//     this.confirmOpen = false;
//     this.pendingAction = null;
//     this.pendingTeam = null;
//     this.pendingMemberId = null;
//     this.cdr.detectChanges();
//   }

//   confirmAction(): void {
//     if (this.processing || this.saving) return;

//     const action = this.pendingAction;
//     const team = this.pendingTeam;
//     const memberId = this.pendingMemberId;

//     this.confirmOpen = false;
//     this.pendingAction = null;
//     this.pendingTeam = null;
//     this.pendingMemberId = null;

//     if (action === 'removeMember' && memberId !== null) {
//       this.removeMemberConfirmed(memberId);
//       return;
//     }

//     if (action === 'save') {
//       this.saveConfirmed();
//       return;
//     }

//     if (!team) return;

//     this.processing = true;

//     if (action === 'status') {
//       const newStatus = team.status === 1 ? 2 : 1;

//       this.api.changeTeamStatus(team.id, newStatus)
//       // this.api.changeTeamStatus(team.id, 1, selectedLeaderId)
//       .subscribe({
//         next: response => {
//           this.processing = false;
//           this.viewOpen = false;
//           this.showToast(response.message);
//           this.load();
//           this.loadSummary();
//         },
//         error: error => {
//           this.processing = false;
//           this.showToast(this.api.errorMessage(error), false);
//         }
//       });
//     }

//     if (action === 'delete') {
//       this.api.deleteTeam(team.id).subscribe({
//         next: response => {
//           this.processing = false;
//           this.formOpen = false;
//           this.editDialog?.hide();
//           this.editingId = null;
//           this.viewOpen = false;
//           this.selectedTeam = null;
//           this.showToast(response.message);
//           this.load();
//           this.loadSummary();
//         },
//         error: error => {
//           this.processing = false;
//           this.showToast(this.api.errorMessage(error), false);
//         }
//       });
//     }
//   }
// }







import { HttpErrorResponse } from '@angular/common/http';

import { CommonModule } from '@angular/common';
import {
  Component,
  ChangeDetectorRef,
  OnInit,
  OnDestroy,
  ViewChild,
  inject
} from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import {
  GridModule,
  GridComponent,
  PageSettingsModel,
    PageService,
  SortService
} from '@syncfusion/ej2-angular-grids';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import {
  TextBoxModule,
  NumericTextBoxModule
} from '@syncfusion/ej2-angular-inputs';
import {
  DialogModule,
  DialogComponent
} from '@syncfusion/ej2-angular-popups';

import {
  Subject,
  Subscription,
  debounceTime,
  distinctUntilChanged
} from 'rxjs';

import { ApiService } from '../../services/api.service';

import {
  MarketingTeam,
  MarketingTeamListItem,
  MarketingTeamRequest,
  Salesman,
  SALESMAN_ROLES
} from '../../models/api.models';

type ConfirmAction =
  'save' | 'delete' | 'status' | 'removeMember';

@Component({
  selector: 'app-marketing-teams',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    GridModule,
    DropDownListModule,
    TextBoxModule,
    NumericTextBoxModule,
    DialogModule
  ],
   providers: [
    PageService,
    SortService
  ],
  templateUrl: './marketing-teams.html',
  styleUrl: './marketing-teams.css'
})
export class MarketingTeams implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild('teamGrid')
  teamGrid?: GridComponent;

  @ViewChild('viewDialog')
  viewDialog?: DialogComponent;

  @ViewChild('editDialog')
  editDialog?: DialogComponent;

  teams: MarketingTeamListItem[] = [];
  gridData = { result: this.teams, count: 0 };
  totalVisibleTeams = 0;
  availableSalesmen: Salesman[] = [];

  readonly roles = SALESMAN_ROLES;

  readonly roleOptions = Object.entries(SALESMAN_ROLES)
    .filter(([key]) => Number(key) !== 1)
    .map(([key, value]) => ({
      key: Number(key),
      value
    }));

  readonly statusOptions = [
    { text: 'Active', value: 1 },
    { text: 'Inactive', value: 2 }
  ];

  readonly pageSettings: PageSettingsModel = {
    pageSize: 10,
    pageSizes: [10, 20, 50]
  };

  search = '';
  selectedStatus: number | null = null;
  readonly filterOptions = [{ text: 'All teams', value: 0 }, { text: 'Active', value: 1 }, { text: 'Inactive', value: 2 }];
  totalTeams = 0;
  activeTeams = 0;
  inactiveTeams = 0;
  readonly maxMembers = 5;
  get memberLimitReached(): boolean { return this.form.members.length >= this.maxMembers; }
  get membershipLocked(): boolean { return this.editingId !== null && this.form.status === 2; }
  loading = false;
  saving = false;
  processing = false;

  // Team details modal
  viewOpen = false;
  viewLoading = false;
  selectedTeam: MarketingTeam | null = null;

  // Create/edit modal
  formOpen = false;
  editingId: number | null = null;
  form: MarketingTeamRequest = this.emptyForm();
  formError = '';

  memberNames: Record<number, string> = {};
  memberCodes: Record<number, string> = {};

  memberSalesmanId: number | null = null;
  memberRole = 3;
  memberTarget = 0;

  // Confirmation modal
  confirmOpen = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmButton = 'Confirm';
  confirmDanger = false;

  pendingAction: ConfirmAction | null = null;
  pendingTeam: MarketingTeamListItem | MarketingTeam | null = null;
  pendingMemberId: number | null = null;

  // Replacement leader activation
  replacementOpen = false;
  replacementTeam: MarketingTeamListItem | MarketingTeam | null = null;
  selectedLeaderId: number | null = null;
  replacementSalesmen: Salesman[] = [];
  replacementLoading = false;
  replacementError = '';

  // Toast
  toastVisible = false;
  toastMessage = '';
  toastSuccess = true;

  private toastTimer?: ReturnType<typeof setTimeout>;
  private readonly searchInput = new Subject<string>();
  private searchSub?: Subscription;
  private listSub?: Subscription;
  private detailSub?: Subscription;

  ngOnInit(): void {
  this.searchSub = this.searchInput.pipe(
    debounceTime(300),
    distinctUntilChanged()
  ).subscribe(() => {
    console.log('Debounced search triggered');
    this.load();
  });

  this.load();
  this.loadSummary();
}

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
    this.listSub?.unsubscribe();
    this.detailSub?.unsubscribe();

    if (this.toastTimer) {
      clearTimeout(this.toastTimer);
    }
  }

  get activeTeamCount(): number {
    return this.activeTeams;
  }

  get inactiveTeamCount(): number {
    return this.inactiveTeams;
  }

  get totalTarget(): number {
    return this.form.members.reduce(
      (sum, member) => sum + Number(member.targetamount || 0),
      0
    );
  }

  get viewTotalTarget(): number {
    return this.selectedTeam?.members.reduce(
      (sum, member) => sum + Number(member.targetamount || 0),
      0
    ) ?? 0;
  }

  // Available unassigned salesmen, plus current team members when editing.
  get leaderOptions() {
    const options = new Map<number, { id: number; name: string }>();
    for (const person of this.availableSalesmen) {
      if (person.status === 1) options.set(person.id, { id: person.id, name: `${person.salesmanname} (${person.salesmancode})` });
    }
    for (const member of this.form.members) {
      if (!options.has(member.salesmanid)) {
        options.set(member.salesmanid, { id: member.salesmanid, name: this.memberName(member.salesmanid) });
      }
    }
    return [...options.values()];
  }

  get selectableSalesmen(): Salesman[] {
    return this.availableSalesmen.filter(person =>
      !this.form.members.some(
        member => member.salesmanid === person.id
      )
    );
  }

  private emptyForm(): MarketingTeamRequest {
    return {
      teamcode: '',
      teamname: '',
      description: null,
      teamleaderid: 0,
      status: 1,
      members: []
    };
  }

  // ==============================
  // NOTIFICATIONS
  // ==============================

  showToast(message: string, success = true): void {
    if (this.toastTimer) {
      clearTimeout(this.toastTimer);
    }

    this.toastMessage = message;
    this.toastSuccess = success;
    this.toastVisible = true;
    this.cdr.detectChanges();

    this.toastTimer = setTimeout(() => {
      this.toastVisible = false;
      this.cdr.detectChanges();
    }, 4000);
  }

  dismissToast(): void {
    this.toastVisible = false;
    this.cdr.detectChanges();
  }

  // ==============================
  // SEARCH AND GRID
  // ==============================


onSearch(value: string | null | undefined): void {
  console.log('SEARCH EVENT:', value);

  this.search = value ?? '';


  this.searchInput.next(this.search);
}

onStatusChange(value: number | string | null | undefined): void {
  

  const status = Number(value);

  this.selectedStatus =
    status === 1 || status === 2 ? status : null;

  this.load();
}


  private loadSummary(): void {
    // These totals must not depend on the search box or status filter.
    this.api.getTeams({ pageSize: 1 }).subscribe({
      next: response => { this.totalTeams = response.totalCount; this.cdr.detectChanges(); },
      error: error => this.showToast(this.api.errorMessage(error), false)
    });
    this.api.getTeams({ status: 1, pageSize: 1 }).subscribe({
      next: response => { this.activeTeams = response.totalCount; this.cdr.detectChanges(); },
      error: error => this.showToast(this.api.errorMessage(error), false)
    });
    this.api.getTeams({ status: 2, pageSize: 1 }).subscribe({
      next: response => { this.inactiveTeams = response.totalCount; this.cdr.detectChanges(); },
      error: error => this.showToast(this.api.errorMessage(error), false)
    });
  }

  
load(requestedPage = 1, requestedPageSize = this.pageSettings.pageSize): void {
  // Cancel the previous API request.
  this.listSub?.unsubscribe();

  this.loading = true;

  const query = {
    search: this.search.trim(),
    status: this.selectedStatus ?? undefined,
    page: requestedPage,
    pageSize: requestedPageSize
  };

  this.listSub = this.api.getTeams(query).subscribe({
    next: response => {
  this.teams = [...response.items];
  this.totalVisibleTeams = response.totalCount;
  this.gridData = {
    result: this.teams,
    count: response.totalCount
  };
  this.loading = false;

  // Update Angular bindings first.
  this.cdr.detectChanges();

  this.cdr.detectChanges();
},

    error: error => {
      this.loading = false;
      this.teams = [];

      this.showToast(
        this.api.errorMessage(error),
        false
      );

      this.cdr.detectChanges();
    }
  });
}

  onGridDataStateChange(event: { skip?: number; take?: number }): void {
    const pageSize = event.take ?? this.pageSettings.pageSize ?? 10;
    const page = Math.floor((event.skip ?? 0) / pageSize) + 1;

    this.load(page, pageSize);
  }

  // ==============================
  // VIEW TEAM
  // ==============================

  openView(team: MarketingTeamListItem): void {
    this.detailSub?.unsubscribe();

    this.viewOpen = true;
    this.viewLoading = true;
    this.selectedTeam = null;
    this.cdr.detectChanges();

    this.detailSub = this.api.getTeam(team.id).subscribe({
      next: detail => {
        this.selectedTeam = detail;
        this.viewLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        this.viewLoading = false;
        this.viewOpen = false;
        this.showToast(this.api.errorMessage(error), false);
      }
    });
  }

  closeView(): void {
    if (this.processing || this.confirmOpen) return;

    this.viewOpen = false;
    this.detailSub?.unsubscribe();
    this.cdr.detectChanges();
  }

  roleName(role: number): string {
    return this.roles[role] ?? `Role ${role}`;
  }

  // ==============================
  // CREATE TEAM
  // ==============================

  openCreate(): void {
    this.editingId = null;
    this.form = this.emptyForm();

    this.memberNames = {};
    this.memberCodes = {};

    this.memberSalesmanId = null;
    this.memberRole = 3;
    this.memberTarget = 0;

    this.formError = '';
    this.viewOpen = false;
    this.formOpen = true;

    this.loadAvailableSalesmen();
    this.cdr.detectChanges();
  }

  // ==============================
  // EDIT TEAM
  // ==============================

  openEdit(team: MarketingTeamListItem | MarketingTeam): void {
    this.api.getTeam(team.id).subscribe({
      next: detail => {
        this.editingId = detail.id;

        this.memberNames = {};
        this.memberCodes = {};

        detail.members.forEach(member => {
          this.memberNames[member.salesmanid] =
            member.salesmanname;
        });

        this.form = {
          teamcode: detail.teamcode,
          teamname: detail.teamname,
          description: detail.description,
          teamleaderid: detail.teamleaderid,
          status: detail.status,
          members: detail.members.map(member => ({
            salesmanid: member.salesmanid,
            role: member.role,
            targetamount: member.targetamount,
            status: member.status
          }))
        };

        this.memberSalesmanId = null;
        this.memberRole = 3;
        this.memberTarget = 0;

        this.formError = '';
        this.viewOpen = false;
        this.formOpen = true;

        this.loadAvailableSalesmen();
        this.cdr.detectChanges();
      },
      error: error => {
        this.showToast(this.api.errorMessage(error), false);
      }
    });
  }

  closeForm(): void {
    if (this.saving || this.confirmOpen) return;

    this.formOpen = false;
    this.editDialog?.hide();
    this.formError = '';
    this.cdr.detectChanges();
  }

  private loadAvailableSalesmen(): void {
    this.api.getAvailableSalesmen().subscribe({
      next: salesmen => {
        this.availableSalesmen = salesmen;
        this.cdr.detectChanges();
      },
      error: error => {
        this.showToast(this.api.errorMessage(error), false);
      }
    });
  }

  memberName(id: number): string {
    return this.memberNames[id] ??
      this.availableSalesmen.find(x => x.id === id)
        ?.salesmanname ??
      `Salesman #${id}`;
  }

  memberCode(id: number): string {
    return this.memberCodes[id] ??
      this.availableSalesmen.find(x => x.id === id)
        ?.salesmancode ??
      `#${id}`;
  }



  
deleteEditingTeam(): void {
  if (this.editingId === null) {
    return;
  }

  // Find the complete team object from the loaded list.
  const team = this.teams.find(
    x => x.id === this.editingId
  );

  if (!team) {
    this.showToast('Marketing team not found.', false);
    return;
  }

  // Open the existing delete confirmation modal.
  this.requestDelete(team);
}


  // ==============================
  // MEMBER MANAGEMENT
  // ==============================

  addMember(): void {
    if (this.membershipLocked) { this.formError = 'This team is inactive. Activate it before adding members.'; this.cdr.detectChanges(); return; }
    if (this.memberLimitReached) { this.formError = 'A team can contain a maximum of 5 members, including the leader.'; this.cdr.detectChanges(); return; }
    const id = Number(this.memberSalesmanId);

    const person = this.selectableSalesmen.find(
      x => x.id === id
    );

    if (!person) {
      this.showToast('Select an available salesman.', false);
      return;
    }

    const isFirst = this.form.members.length === 0;

    this.memberNames[id] = person.salesmanname;
    this.memberCodes[id] = person.salesmancode;

    this.form.members = [
      ...this.form.members,
      {
        salesmanid: id,
        role: isFirst ? 1 : Number(this.memberRole),
        targetamount: Number(this.memberTarget) || 0,
        status: 1
      }
    ];

    if (isFirst) {
      this.form.teamleaderid = id;
    }

    this.memberSalesmanId = null;
    this.memberRole = 3;
    this.memberTarget = 0;

    this.cdr.detectChanges();
  }

  setLeader(id: number): void {
    const leaderId = Number(id);
    if (!leaderId) return;
    if (this.membershipLocked) {
      this.formError = 'Activate this team before changing its leader.';
      this.cdr.detectChanges();
      return;
    }
    const alreadyMember = this.form.members.some(m => m.salesmanid === leaderId);
    if (!alreadyMember) {
      if (this.memberLimitReached) {
        this.formError = 'Maximum 5 members including the leader. Remove a member first.';
        this.cdr.detectChanges();
        return;
      }
      const person = this.availableSalesmen.find(p => p.id === leaderId);
      if (!person) {
        this.formError = 'Select an available, active salesman.';
        this.cdr.detectChanges();
        return;
      }
      this.memberNames[leaderId] = person.salesmanname;
      this.memberCodes[leaderId] = person.salesmancode;
      this.form.members = [...this.form.members, {
        salesmanid: leaderId, role: 1, targetamount: 0, status: 1
      }];
    }
    this.form.teamleaderid = leaderId;
    this.form.members = this.form.members.map(member => ({
      ...member,
      role: member.salesmanid === leaderId ? 1 : (member.role === 1 ? 2 : member.role)
    }));
    this.formError = '';
    this.cdr.detectChanges();
  }

  requestRemoveMember(id: number): void {
    if (this.membershipLocked) { this.formError = 'Activate this team before changing its members.'; this.cdr.detectChanges(); return; }
    if (this.form.teamleaderid === id) {
      this.showToast(
        'Select another team leader before removing this member.',
        false
      );
      return;
    }

    this.pendingMemberId = id;
    this.pendingAction = 'removeMember';
    this.confirmTitle = 'Remove Team Member';
    this.confirmMessage =
      `Remove ${this.memberName(id)} from the team? ` +
      'This change will take effect after saving.';
    this.confirmButton = 'Remove Member';
    this.confirmDanger = true;
    this.confirmOpen = true;
    this.cdr.detectChanges();
  }

  private removeMemberConfirmed(id: number): void {
    this.form.members = this.form.members.filter(
      member => member.salesmanid !== id
    );

    this.showToast(
      'Member removed from the form. Save to apply changes.'
    );
    this.cdr.detectChanges();
  }

  // ==============================
  // SAVE
  // ==============================

  requestSave(): void {
    this.formError = '';

    if (!this.form.teamcode.trim()) {
      this.formError = 'Team code is required.';
    } else if (!this.form.teamname.trim()) {
      this.formError = 'Team name is required.';
    } else if (this.form.members.length > this.maxMembers) {
      this.formError = 'A team can contain a maximum of 5 members.';
    } else if (!this.form.members.length) {
      this.formError = 'Add at least one team member.';
    } else if (!this.form.teamleaderid ||
      !this.form.members.some(
        m => m.salesmanid === Number(this.form.teamleaderid)
      )) {
      this.formError = 'Select a valid team leader.';
    } else if (this.form.members.some(
      m => Number(m.targetamount) < 0
    )) {
      this.formError = 'Target amount cannot be negative.';
    }

    if (this.formError) {
      this.showToast(this.formError, false);
      return;
    }

    this.pendingAction = 'save';
    this.confirmTitle = this.editingId === null
      ? 'Create Marketing Team'
      : 'Update Marketing Team';
    this.confirmMessage =
      'Are you sure you want to save these team details?';
    this.confirmButton = this.editingId === null
      ? 'Create Team'
      : 'Update Team';
    this.confirmDanger = false;
    this.confirmOpen = true;
    this.cdr.detectChanges();
  }

  private saveConfirmed(): void {
    if (this.saving) return;

    this.setLeader(this.form.teamleaderid);
    this.saving = true;

    const request: MarketingTeamRequest = {
      ...this.form,
      members: this.form.members.map(m => ({ ...m }))
    };

    const id = this.editingId;

    const success = () => {
      this.saving = false;
      this.formOpen = false;
      this.editDialog?.hide();
      this.editingId = null;
      this.formError = '';
      this.cdr.detectChanges();

      this.showToast(
        id === null
          ? 'Marketing team created successfully.'
          : 'Marketing team updated successfully.'
      );

      this.load();
      this.loadSummary();
    };

    const failure = (error: unknown) => {
      this.saving = false;
      this.formError = this.api.errorMessage(error);
      this.showToast(this.formError, false);
    };

    if (id === null) {
      this.api.createTeam(request).subscribe({
        next: success,
        error: failure
      });
    } else {
      this.api.updateTeam(id, request).subscribe({
        next: success,
        error: failure
      });
    }
  }

  // ==============================
  // STATUS
  // ==============================

  requestStatus(team: MarketingTeamListItem | MarketingTeam): void {
    const activate = team.status !== 1;

    this.pendingTeam = team;
    this.pendingAction = 'status';

    this.confirmTitle = activate
      ? 'Activate Marketing Team'
      : 'Deactivate Marketing Team';

    this.confirmMessage =
      `Are you sure you want to ${activate ? 'activate' : 'deactivate'} ` +
      `${team.teamname}?`;

    this.confirmButton = activate ? 'Activate' : 'Deactivate';
    this.confirmDanger = !activate;
    this.confirmOpen = true;
    this.cdr.detectChanges();
  }

  // ==============================
  // DELETE
  // ==============================

  requestDelete(team: MarketingTeamListItem | MarketingTeam): void {
    if (team.status === 1) {
      this.showToast('Deactivate team first to delete', false);
      return;
    }
    if (team.status !== 2) {
      this.showToast('Only inactive teams can be deleted.', false);
      return;
    }

    this.pendingTeam = team;
    this.pendingAction = 'delete';
    this.confirmTitle = 'Delete Marketing Team';
    this.confirmMessage =
      `Are you sure you want to delete ${team.teamname}? ` +
      'The team will be marked as deleted and its history preserved.';
    this.confirmButton = 'Delete Team';
    this.confirmDanger = true;
    this.confirmOpen = true;
    this.cdr.detectChanges();
  }

  openReplacementDialog(team: MarketingTeamListItem | MarketingTeam): void {
    this.replacementTeam = team;
    this.selectedLeaderId = null;
    this.replacementSalesmen = [];
    this.replacementError = '';
    this.replacementLoading = true;
    this.replacementOpen = true;
    this.cdr.detectChanges();

    this.api.getAvailableSalesmen().subscribe({
      next: salesmen => {
        // Exclude the current leader; the backend already reported
        // that this person is unavailable for activation.
        this.replacementSalesmen = salesmen.filter(person =>
          person.status === 1 &&
          person.id !== this.replacementTeam?.teamleaderid
        );
        this.replacementLoading = false;
        this.cdr.detectChanges();
      },
      error: error => {
        this.replacementLoading = false;
        this.replacementError = this.api.errorMessage(error);
        this.cdr.detectChanges();
      }
    });
  }

  closeReplacementDialog(): void {
    if (this.processing) return;
    this.replacementOpen = false;
    this.replacementTeam = null;
    this.selectedLeaderId = null;
    this.replacementError = '';
    this.cdr.detectChanges();
  }

  activateWithNewLeader(): void {
    if (this.processing || this.replacementLoading) return;

    const team = this.replacementTeam;
    const leaderId = Number(this.selectedLeaderId);
    if (!team || !leaderId) {
      this.replacementError = 'Please select a new Team Leader.';
      this.cdr.detectChanges();
      return;
    }
    if (!this.replacementSalesmen.some(person => person.id === leaderId)) {
      this.replacementError = 'Please select an available salesman.';
      this.cdr.detectChanges();
      return;
    }

    this.processing = true;
    this.replacementError = '';
    this.api.changeTeamStatus(team.id, 1, leaderId).subscribe({
      next: response => {
        this.processing = false;
        this.replacementOpen = false;
        this.replacementTeam = null;
        this.selectedLeaderId = null;
        this.viewOpen = false;
        this.showToast(response.message);
        this.load();
        this.loadSummary();
      },
      error: error => {
        this.processing = false;
        this.replacementError = this.api.errorMessage(error);
        this.selectedLeaderId = null;
        this.api.getAvailableSalesmen().subscribe({
          next: salesmen => {
            this.replacementSalesmen = salesmen.filter(person =>
              person.status === 1 &&
              person.id !== this.replacementTeam?.teamleaderid
            );
            this.cdr.detectChanges();
          },
          error: () => this.cdr.detectChanges()
        });
        this.cdr.detectChanges();
      }
    });
  }

  // ==============================
  // CONFIRMATION
  // ==============================

  closeConfirmation(): void {
    if (this.processing || this.saving) return;

    this.confirmOpen = false;
    this.pendingAction = null;
    this.pendingTeam = null;
    this.pendingMemberId = null;
    this.cdr.detectChanges();
  }

  confirmAction(): void {
    if (this.processing || this.saving) return;

    const action = this.pendingAction;
    const team = this.pendingTeam;
    const memberId = this.pendingMemberId;

    this.confirmOpen = false;
    this.pendingAction = null;
    this.pendingTeam = null;
    this.pendingMemberId = null;

    if (action === 'removeMember' && memberId !== null) {
      this.removeMemberConfirmed(memberId);
      return;
    }

    if (action === 'save') {
      this.saveConfirmed();
      return;
    }

    if (!team) return;

    this.processing = true;

    if (action === 'status') {
      const newStatus = team.status === 1 ? 2 : 1;

      this.api.changeTeamStatus(team.id, newStatus).subscribe({
        next: response => {
          this.processing = false;
          this.viewOpen = false;
          this.showToast(response.message);
          this.load();
          this.loadSummary();
        },
        error: error => {
          this.processing = false;
          const message = this.api.errorMessage(error);
          const leaderUnavailable =
            error instanceof HttpErrorResponse &&
            error.status === 409 &&
            message.toLowerCase().includes('current team leader is unavailable');

          if (newStatus === 1 && leaderUnavailable) {
            this.openReplacementDialog(team);
            return;
          }
          this.showToast(message, false);
        }
      });
    }

    if (action === 'delete') {
      this.api.deleteTeam(team.id).subscribe({
        next: response => {
          this.processing = false;
          this.formOpen = false;
          this.editDialog?.hide();
          this.editingId = null;
          this.viewOpen = false;
          this.selectedTeam = null;
          this.showToast(response.message);
          this.load();
          this.loadSummary();
        },
        error: error => {
          this.processing = false;
          this.showToast(this.api.errorMessage(error), false);
        }
      });
    }
  }
}
