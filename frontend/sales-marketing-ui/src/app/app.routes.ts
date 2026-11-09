import { Routes } from '@angular/router';

export const routes: Routes = [
     {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () =>                     //enables lazy loading.
      import('./pages/login/login')
        .then(m => m.Login)
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./pages/dashboard/dashboard')
        .then(m => m.Dashboard)
  },
  {
    path: 'salesmen',
    loadComponent: () =>
      import('./pages/salesmen/salesmen')
        .then(m => m.Salesmen)
  },
  {
    path: 'marketing-teams',
    loadComponent: () =>
      import('./pages/marketing-teams/marketing-teams')
        .then(m => m.MarketingTeams)
  },
  {
    path: '**',                      // The ** route is a wildcard route. It redirects unknown URLs to the login page.
    redirectTo: 'login'
  }
];
