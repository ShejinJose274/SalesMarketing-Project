
import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { RouterLink } from '@angular/router';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';

import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

import {
  MarketingTeamListItem,
  Salesman
} from '../../models/api.models';

@Component({
  selector: 'app-dashboard',
  imports: [
    RouterLink,
    ButtonModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  // Dependency injection
  private readonly cdr = inject(ChangeDetectorRef);
  readonly auth = inject(AuthService);
  private readonly api = inject(ApiService);

  // Dashboard data
  salesmen: Salesman[] = [];
  teams: MarketingTeamListItem[] = [];

  // UI state
  loading = true;
  error = '';

  // Load dashboard data
  ngOnInit() {
    this.api.getSalesmen({ pageSize: 100 }).subscribe({
      next: (response) => {
        this.salesmen = response.items;

        // Load marketing teams if user is MasterAdmin
        this.loadTeams();

        // Update dashboard immediately
        this.cdr.detectChanges();
      },

      error: (error) => {
        this.error = this.api.errorMessage(error);
        this.loading = false;

        // Update error message immediately
        this.cdr.detectChanges();
      }
    });
  }

  // Load marketing teams
  private loadTeams() {

    // SalesManagerAdmin does not need team data
    if (!this.auth.hasRole('MasterAdmin')) {
      this.loading = false;

      this.cdr.detectChanges();
      return;
    }

    this.api.getTeams({ pageSize: 100 }).subscribe({
      next: (response) => {
        this.teams = response.items;
        this.loading = false;

        // Update dashboard after teams load
        this.cdr.detectChanges();
      },

      error: (error) => {
        this.error = this.api.errorMessage(error);
        this.loading = false;

        // Update error message immediately
        this.cdr.detectChanges();
      }
    });
  }

  // Dashboard statistics
  get activeSalesmen() {
    return this.salesmen.filter(
      x => x.status === 1
    ).length;
  }

  get inactiveSalesmen() {
    return this.salesmen.filter(
      x => x.status === 2
    ).length;
  }

  get activeTeams() {
    return this.teams.filter(
      x => x.status === 1
    ).length;
  }
}
