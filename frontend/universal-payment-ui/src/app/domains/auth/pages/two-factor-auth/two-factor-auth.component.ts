import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

import { AuthService } from '../../../../core/authentication/auth.service';

@Component({
  selector: 'app-two-factor-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './two-factor-auth.component.html',
  styleUrls: ['./two-factor-auth.component.css']
})
export class TwoFactorAuthComponent implements OnInit, OnDestroy {
  setupForm: FormGroup;
  disableForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  qrCodeUrl = '';
  secret = '';
  recoveryCodes: string[] = [];
  showSetupForm = false;
  showRecoveryCodes = false;
  
  // Public property to access in template
  twoFactorEnabled = false;
  
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    public authService: AuthService, // Changed to public for template access
    private router: Router
  ) {
    this.setupForm = this.createSetupForm();
    this.disableForm = this.createDisableForm();
  }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    // Set the initial state based on user's 2FA status
    this.twoFactorEnabled = this.authService.currentUserValue?.twoFactorEnabled || false;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createSetupForm(): FormGroup {
    return this.formBuilder.group({
      verificationCode: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(6),
        Validators.pattern(/^[0-9]*$/)
      ]]
    });
  }

  private createDisableForm(): FormGroup {
    return this.formBuilder.group({
      verificationCode: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(6),
        Validators.pattern(/^[0-9]*$/)
      ]]
    });
  }

  get setupVerificationCode() {
    return this.setupForm.get('verificationCode');
  }

  get disableVerificationCode() {
    return this.disableForm.get('verificationCode');
  }

  startSetup(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.setupTwoFactor()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          this.qrCodeUrl = response.qrCode;
          this.secret = response.secret;
          this.recoveryCodes = response.recoveryCodes;
          this.showSetupForm = true;
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Failed to setup two-factor authentication. Please try again.';
        }
      });
  }

  enableTwoFactor(): void {
    if (this.setupForm.invalid || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const verificationCode = this.setupForm.value.verificationCode;

    this.authService.enableTwoFactor(verificationCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success) {
            this.successMessage = 'Two-factor authentication has been enabled successfully!';
            this.showRecoveryCodes = true;
            this.showSetupForm = false;
            this.twoFactorEnabled = true;
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Failed to enable two-factor authentication. Please check your verification code and try again.';
        }
      });
  }

  disableTwoFactor(): void {
    if (this.disableForm.invalid || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const verificationCode = this.disableForm.value.verificationCode;

    this.authService.disableTwoFactor(verificationCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success) {
            this.successMessage = 'Two-factor authentication has been disabled successfully.';
            this.showSetupForm = false;
            this.showRecoveryCodes = false;
            this.twoFactorEnabled = false;
            this.disableForm.reset();
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Failed to disable two-factor authentication. Please check your verification code and try again.';
        }
      });
  }

  cancelSetup(): void {
    this.showSetupForm = false;
    this.showRecoveryCodes = false;
    this.setupForm.reset();
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Add the missing copySecret method
  copySecret(): void {
    navigator.clipboard.writeText(this.secret).then(() => {
      this.successMessage = 'Secret key copied to clipboard!';
      // Clear success message after 3 seconds
      setTimeout(() => {
        if (this.successMessage === 'Secret key copied to clipboard!') {
          this.successMessage = '';
        }
      }, 3000);
    }).catch(() => {
      this.errorMessage = 'Failed to copy secret key to clipboard.';
    });
  }

  copyRecoveryCodes(): void {
    const codesText = this.recoveryCodes.join('\n');
    navigator.clipboard.writeText(codesText).then(() => {
      this.successMessage = 'Recovery codes copied to clipboard!';
    }).catch(() => {
      this.errorMessage = 'Failed to copy recovery codes to clipboard.';
    });
  }

  downloadRecoveryCodes(): void {
    const codesText = this.recoveryCodes.join('\n');
    const blob = new Blob([codesText], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'recovery-codes.txt';
    link.click();
    window.URL.revokeObjectURL(url);
  }

  onBack(): void {
    this.router.navigate(['/']);
  }
}