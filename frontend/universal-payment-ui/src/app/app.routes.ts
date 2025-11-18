import { Routes } from '@angular/router';
import { Component } from '@angular/core';
import { AuthGuard } from './core/authentication/auth.guard';

@Component({
  selector: 'app-login',
  template: '<p>Login</p>'
})
export class LoginComponent {}

@Component({
  selector: 'app-dashboard',
  template: '<p>Dashboard</p>'
})
export class DashboardComponent {}

export const routes: Routes = [
  // Login page (public)
  { path: 'auth/login', component: LoginComponent },

  // Dashboard (protected)
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },

  // Redirect empty path to login
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Wildcard for 404
  { path: '**', redirectTo: 'auth/login' }
];
