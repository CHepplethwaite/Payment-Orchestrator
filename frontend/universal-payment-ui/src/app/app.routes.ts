import { Routes } from '@angular/router';
import { AuthGuard } from './core/authentication/auth.guard';
import { LoginComponent } from './domains/auth/pages/login/login.component';
import { DashboardComponent } from './domains/dashboard/dashboard.component';

export const routes: Routes = [
  // Public routes
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },

  // Protected routes (lazy-loaded feature modules)
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },

  {
    path: 'payments',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/payments/payments.module').then(m => m.PaymentsModule)
  },

  {
    path: 'kyc',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/kyc/kyc.module').then(m => m.KycModule)
  },

  {
    path: 'admin',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/admin/admin.module').then(m => m.AdminModule)
  },

  {
    path: 'agents',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/agents/agents.module').then(m => m.AgentsModule)
  },

  {
    path: 'merchants',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/merchants/merchants.module').then(m => m.MerchantsModule)
  },

  {
    path: 'reports',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/reports/reports.module').then(m => m.ReportsModule)
  },

  {
    path: 'webhooks',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/webhooks/webhooks.module').then(m => m.WebhooksModule)
  },

  // Provider-specific routes
  {
    path: 'providers/airtel',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/providers/airtel/airtel.module').then(m => m.AirtelModule)
  },

  {
    path: 'providers/mtn',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/providers/mtn/mtn.module').then(m => m.MtnModule)
  },

  {
    path: 'providers/zamtel',
    canActivate: [AuthGuard],
    loadChildren: () =>
      import('./domains/providers/zamtel/zamtel.module').then(m => m.ZamtelModule)
  },

  // Default redirects
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' }
];