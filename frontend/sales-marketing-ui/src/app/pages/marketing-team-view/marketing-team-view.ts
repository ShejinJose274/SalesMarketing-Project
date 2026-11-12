
// import { CommonModule } from '@angular/common';
// import {
//   Component,
//   ChangeDetectorRef,
//   OnInit,
//   inject
// } from '@angular/core';
// import { ActivatedRoute, RouterLink } from '@angular/router';

// import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
// import { GridModule } from '@syncfusion/ej2-angular-grids';

// import { ApiService } from '../../services/api.service';
// import {
//   MarketingTeam,
//   SALESMAN_ROLES
// } from '../../models/api.models';

// @Component({
//   selector: 'app-marketing-team-view',
//   standalone: true,
//   imports: [
//     CommonModule,
//     RouterLink,
//     ButtonModule,
//     GridModule
//   ],
//   templateUrl: './marketing-team-view.html',
//   styleUrl: './marketing-team-view.css'
// })
// export class MarketingTeamView implements OnInit {
//   private readonly route = inject(ActivatedRoute);
//   private readonly api = inject(ApiService);
//   private readonly cdr = inject(ChangeDetectorRef);

//   readonly roles = SALESMAN_ROLES;

//   team: MarketingTeam | null = null;
//   loading = true;
//   error = '';

//   ngOnInit(): void {
//     const id = Number(this.route.snapshot.paramMap.get('id'));

//     if (!id) {
//       this.error = 'Invalid marketing team ID.';
//       this.loading = false;
//       return;
//     }

//     this.api.getTeam(id).subscribe({
//       next: team => {
//         this.team = team;
//         this.loading = false;
//         this.cdr.detectChanges();
//       },
//       error: error => {
//         this.error = this.api.errorMessage(error);
//         this.loading = false;
//         this.cdr.detectChanges();
//       }
//     });
//   }

//   get totalTarget(): number {
//     return this.team?.members.reduce(
//       (sum, member) => sum + Number(member.targetamount || 0),
//       0
//     ) ?? 0;
//   }

//   roleName(role: number): string {
//     return this.roles[role] ?? `Role ${role}`;
//   }
// }





import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-marketing-team-view',
  standalone: true,
  templateUrl: './marketing-team-view.html',
  styleUrl: './marketing-team-view.css'
})
export class MarketingTeamView implements OnInit {
  private readonly router = inject(Router);

  ngOnInit(): void {
    void this.router.navigateByUrl('/marketing-team', {
      replaceUrl: true
    });
  }
}
