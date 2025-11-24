import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

// Auth service and types - adjust import path as needed
import { AuthService } from '../../../../core/authentication/auth.service';

@Component({
  selector: 'app-login-2fa',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login-2fa.component.html',
  styleUrls: ['./login-2fa.component.css']
})
export class Login2faComponent implements OnInit, OnDestroy {
  twoFactorForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  showBackupCodeInput = false;
  isResendingCode = false;
  
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.twoFactorForm = this.createForm();
  }

  ngOnInit(): void {
    // Check if user is already authenticated (shouldn't be here if they are)
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
      return;
    }

    // Listen for query parameters (like returnUrl)
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        // You can handle any query parameters here if needed
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createForm(): FormGroup {
    return this.formBuilder.group({
      code: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(6),
        Validators.pattern(/^[0-9]*$/)
      ]],
      rememberDevice: [false]
    });
  }

  get code() {
    return this.twoFactorForm.get('code');
  }

  get rememberDevice() {
    return this.twoFactorForm.get('rememberDevice');
  }

  onSubmit(): void {
    if (this.twoFactorForm.invalid || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { code, rememberDevice } = this.twoFactorForm.value;

    this.authService.verifyTwoFactorCode(code, rememberDevice)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          
          // Check if there's a return URL or redirect to dashboard
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
          this.router.navigate([returnUrl]);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Two-factor authentication failed. Please try again.';
          
          // Clear the form on error
          this.code?.setValue('');
          this.code?.markAsUntouched();
        }
      });
  }

  onBackupCodeSubmit(backupCode: string): void {
    if (!backupCode || this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.verifyBackupCode(backupCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
          this.router.navigate([returnUrl]);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.message || 'Invalid backup code. Please try again.';
        }
      });
  }

  resendCode(): void {
    if (this.isResendingCode) {
      return;
    }

    this.isResendingCode = true;
    this.errorMessage = '';

    this.authService.resendTwoFactorCode()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isResendingCode = false;
          // Show success message
          this.errorMessage = 'Verification code has been resent to your authenticator app.';
          
          // Clear success message after 5 seconds
          setTimeout(() => {
            if (this.errorMessage === 'Verification code has been resent to your authenticator app.') {
              this.errorMessage = '';
            }
          }, 5000);
        },
        error: (error) => {
          this.isResendingCode = false;
          this.errorMessage = error.message || 'Failed to resend verification code. Please try again.';
        }
      });
  }

  toggleBackupCodeInput(): void {
    this.showBackupCodeInput = !this.showBackupCodeInput;
    this.errorMessage = '';
    
    if (this.showBackupCodeInput) {
      this.twoFactorForm.get('code')?.clearValidators();
    } else {
      this.twoFactorForm.get('code')?.setValidators([
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(6),
        Validators.pattern(/^[0-9]*$/)
      ]);
    }
    
    this.twoFactorForm.get('code')?.updateValueAndValidity();
  }

  onBackToLogin(): void {
    this.authService.logout('/auth/login');
  }

  // Helper method to get input classes based on validation state
  getInputClasses(field: any): { [key: string]: boolean } {
    return {
      'input-valid': field.valid && (field.dirty || field.touched),
      'input-invalid': field.invalid && (field.dirty || field.touched)
    };
  }
}