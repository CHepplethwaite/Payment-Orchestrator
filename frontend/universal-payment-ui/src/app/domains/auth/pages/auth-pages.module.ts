import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

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

import { SharedModule } from '../../../shared/shared.module';

@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    VerifyEmailComponent,
    ResendVerificationComponent,
    TwoFactorAuthComponent,
    LockAccountComponent,
    LogoutComponent,
    OauthCallbackComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    SharedModule
  ],
  exports: [
    LoginComponent,
    RegisterComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    VerifyEmailComponent
  ]
})
export class AuthPagesModule { }