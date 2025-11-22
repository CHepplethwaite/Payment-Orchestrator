import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { AUTH_SERVICE } from '../../../../core/tokens/auth-service.token';
import { NotificationService } from '../../../../core/services/notification/notification.service';

// Interface for password strength
interface PasswordStrength {
  strength: string;
  color: string;
  percentage: number;
}

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class ResetPasswordComponent implements OnInit, OnDestroy {
  resetPasswordForm!: FormGroup;
  loading = false;
  showPassword = false;
  showConfirmPassword = false;
  token: string = '';
  email: string = '';
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    @Inject(AUTH_SERVICE) private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.token = this.route.snapshot.queryParams['token'] || '';
    this.email = this.route.snapshot.queryParams['email'] || '';

    if (!this.token || !this.email) {
      this.notificationService.showError('Invalid reset password link');
      this.router.navigate(['/auth/forgot-password']);
      return;
    }

    this.resetPasswordForm = this.formBuilder.group({
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // Safe getter for form controls with proper typing
  get f(): { [key: string]: AbstractControl } {
    return this.resetPasswordForm.controls;
  }

  // Password match validator with proper typing
  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      if (confirmPassword && confirmPassword.errors?.['passwordMismatch']) {
        const errors = { ...confirmPassword.errors };
        delete errors['passwordMismatch'];
        confirmPassword.setErrors(Object.keys(errors).length ? errors : null);
      }
    }
    return null;
  }

  // Password strength calculation
  getPasswordStrength(): PasswordStrength {
    const password = this.resetPasswordForm.get('password')?.value || '';
    
    if (!password) {
      return { strength: '', color: '#ddd', percentage: 0 };
    }

    let strength = 0;

    // Length check
    if (password.length >= 8) strength += 25;
    if (password.length >= 12) strength += 10;

    // Character type checks
    if (/[a-z]/.test(password)) strength += 15;
    if (/[A-Z]/.test(password)) strength += 15;
    if (/[0-9]/.test(password)) strength += 15;
    if (/[@$!%*?&]/.test(password)) strength += 20;

    // Determine strength level
    let strengthText = '';
    let color = '';

    if (strength < 40) {
      strengthText = 'Weak';
      color = '#dc3545';
    } else if (strength < 70) {
      strengthText = 'Fair';
      color = '#fd7e14';
    } else if (strength < 90) {
      strengthText = 'Good';
      color = '#ffc107';
    } else {
      strengthText = 'Strong';
      color = '#28a745';
    }

    return { strength: strengthText, color, percentage: Math.min(strength, 100) };
  }

  // Get password strength percentage for progress bar
  getPasswordStrengthPercentage(): number {
    return this.getPasswordStrength().percentage;
  }

  onSubmit() {
    if (this.resetPasswordForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    
    const password = this.resetPasswordForm.get('password')?.value;
    
    if (!password) {
      this.loading = false;
      return;
    }

    this.authService.resetPassword(this.token, password)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          this.loading = false;
          this.notificationService.showSuccess('Password reset successfully! You can now login with your new password.');
          this.router.navigate(['/auth/login']);
        },
        error: (error: Error) => {
          this.loading = false;
          this.notificationService.showError(error.message || 'Failed to reset password');
        }
      });
  }

  private markFormGroupTouched() {
    if (this.resetPasswordForm) {
      this.resetPasswordForm.markAllAsTouched();
    }
  }

  togglePasswordVisibility(field: 'password' | 'confirmPassword') {
    if (field === 'password') {
      this.showPassword = !this.showPassword;
    } else {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  // Navigation method
  navigateToLogin() {
    this.router.navigate(['/auth/login']);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}