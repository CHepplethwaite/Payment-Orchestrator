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
import { Login2faComponent } from './domains/auth/pages/login-2fa/login-2fa.component';
import { OauthCallbackComponent } from './domains/auth/pages/oauth-callback/oauth-callback.component';

import { AuthGuard } from './core/authentication/auth.guard';
import { GuestGuard } from './core/authentication/guest.guard';
import { LockGuard } from './core/authentication/lock.guard'; // optional guard for locked accounts

export const routes: Routes = [
  {
    path: 'auth',
    children: [
      // Guest routes
      { path: 'login', component: LoginComponent, canActivate: [GuestGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [GuestGuard] },
      { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [GuestGuard] },
      { path: 'reset-password/:token', component: ResetPasswordComponent, canActivate: [GuestGuard] },
      { path: 'verify-email/:token', component: VerifyEmailComponent, canActivate: [GuestGuard] },
      { path: 'resend-verification', component: ResendVerificationComponent, canActivate: [GuestGuard] },

      // Locked account
      { path: 'lock', component: LockAccountComponent, canActivate: [LockGuard] },

      // 2FA routes
      { path: '2fa', component: TwoFactorAuthComponent, canActivate: [AuthGuard] },
      { path: 'login-2fa', component: Login2faComponent, canActivate: [GuestGuard] },

      // OAuth callback with provider param
      {
        path: 'oauth',
        children: [
          { path: 'callback/:provider', component: OauthCallbackComponent, canActivate: [GuestGuard] }
        ]
      },

      // Authenticated route
      { path: 'logout', component: LogoutComponent, canActivate: [AuthGuard] },

      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },

  // Default redirect
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Catch-all redirect
  { path: '**', redirectTo: 'auth/login', pathMatch: 'full' }
];
