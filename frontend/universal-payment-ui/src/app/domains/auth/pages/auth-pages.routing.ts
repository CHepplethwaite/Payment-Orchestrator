import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { VerifyEmailComponent } from './verify-email/verify-email.component';
import { ResendVerificationComponent } from './resend-verification/resend-verification.component';
import { TwoFactorAuthComponent } from './two-factor-auth/two-factor-auth.component';
import { LockAccountComponent } from './lock-account/lock-account.component';
import { LogoutComponent } from './logout/logout.component';
import { OauthCallbackComponent } from './oauth-callback/oauth-callback.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },
  { path: 'verify-email', component: VerifyEmailComponent },
  { path: 'resend-verification', component: ResendVerificationComponent },
  { path: 'two-factor', component: TwoFactorAuthComponent },
  { path: 'lock-account', component: LockAccountComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'oauth-callback', component: OauthCallbackComponent },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AuthPagesRoutingModule { }