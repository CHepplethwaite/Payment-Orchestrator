import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../core/services/notification/notification.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
  standalone: true
})
export class ForgotPasswordComponent implements OnInit, OnDestroy {
  forgotPasswordForm!: FormGroup;
  loading = false;
  emailSent = false;
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.forgotPasswordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get f() { return this.forgotPasswordForm.controls; }

  onSubmit() {
    if (this.forgotPasswordForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.authService.forgotPassword(this.f['email'].value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.emailSent = true;
          this.notificationService.showSuccess('Password reset instructions sent to your email');
          this.loading = false;
        },
        error: (error) => {
          this.notificationService.showError(error.message || 'Failed to send reset instructions');
          this.loading = false;
        }
      });
  }

  private markFormGroupTouched() {
    // Add null check and use optional chaining
    if (!this.forgotPasswordForm) return;
    
    Object.keys(this.forgotPasswordForm.controls).forEach(key => {
      const control = this.forgotPasswordForm.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }

  navigateToLogin() {
    this.router.navigate(['/auth/login']);
  }

  resendInstructions() {
    this.emailSent = false;
    this.forgotPasswordForm.reset();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}