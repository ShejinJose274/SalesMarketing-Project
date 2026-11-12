import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './guards/auth.guard';
import { Login } from './pages/login/login';
import { Dashboard } from './pages/dashboard/dashboard';
import { MarketingTeams } from './pages/marketing-teams/marketing-teams';
import { Salesmen } from './pages/salesmen/salesmen';
import { Shell } from './pages/shell/shell';
import { MarketingTeamView } from './pages/marketing-team-view/marketing-team-view';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: '', component: Shell, canActivate: [authGuard], children: [
    { path: 'dashboard', component: Dashboard },
    { path: 'salesman', component: Salesmen },
    { path: 'salesman/new', component: Salesmen },
    { path: 'salesman/:id/edit', component: Salesmen },
    { path: 'salesmen', component: Salesmen },
    { path: 'marketing-team', component: MarketingTeams, canActivate: [roleGuard('MasterAdmin')] },
    { path: 'marketing-team/new', component: MarketingTeams, canActivate: [roleGuard('MasterAdmin')] },
    { path: 'marketing-team/:id/edit', component: MarketingTeams, canActivate: [roleGuard('MasterAdmin')] },
    { path: 'marketing-team/:id', component: MarketingTeamView, canActivate: [roleGuard('MasterAdmin')] },
    { path: 'marketing-teams', component: MarketingTeams, canActivate: [roleGuard('MasterAdmin')] },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  ] },
  { path: '**', redirectTo: '' },
];
