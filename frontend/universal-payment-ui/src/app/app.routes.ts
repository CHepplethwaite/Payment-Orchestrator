import { Routes } from '@angular/router';
import { LoginComponent } from './domains/auth/pages/login/login.component';
import { RegisterComponent } from './domains/auth/pages/register/register.component';
import { ForgotPasswordComponent } from './domains/auth/pages/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './domains/auth/pages/reset-password/reset-password.component';
import { VerifyEmailComponent } from './domains/auth/pages/verify-email/verify-email.component';
import { LogoutComponent } from './domains/auth/pages/logout/logout.component';
import { ResendVerificationComponent } from './domains/auth/pages/resend-verification/resend-verification.component';
import { LockAccountComponent } from './domains/auth/pages/lock-account/lock-account.component';
import { TwoFactorAuthComponent } from './domains/auth/pages/two-factor-auth/two-factor-auth.component';
import { OAuthCallbackComponent } from './domains/auth/pages/oauth-callback/oauth-callback.component';

import { AuthGuard } from './auth/guards/auth.guard';      // protects routes for logged-in users
import { GuestGuard } from './auth/guards/guest.guard';    // protects routes for guests only

export const routes: Routes = [
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent, canActivate: [GuestGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [GuestGuard] },
      { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [GuestGuard] },
      { path: 'reset-password/:token', component: ResetPasswordComponent, canActivate: [GuestGuard] },
      { path: 'verify-email/:token', component: VerifyEmailComponent, canActivate: [GuestGuard] },
      { path: 'resend-verification', component: ResendVerificationComponent, canActivate: [GuestGuard] },
      { path: 'lock', component: LockAccountComponent },
      { path: '2fa', component: TwoFactorAuthComponent, canActivate: [AuthGuard] },
      { path: 'oauth/callback', component: OAuthCallbackComponent, canActivate: [GuestGuard] },
      { path: 'logout', component: LogoutComponent, canActivate: [AuthGuard] },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },

  // Optional: default redirect to login
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Wildcard route for unknown paths
  { path: '**', redirectTo: 'auth/login' }
];
