import { Routes } from '@angular/router';
import { LoginComponent } from './domains/auth/pages/login/login.component';

export const routes: Routes = [
  { path: 'auth/login', component: LoginComponent },
  { path: 'dashboard', redirectTo: 'auth/login' },
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: 'auth/login' }
];
