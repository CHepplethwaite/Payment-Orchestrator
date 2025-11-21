import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthenticationService } from '../../../core/services/authentication.service';
import { NotificationService } from '../../../../shared/services/notification.service';

@Component({
  selector: 'app-resend-verification',
  templateUrl: './resend-verification.component.html',
  styleUrls: ['./resend-verification.component.scss']
})
export class ResendVerificationComponent implements OnInit, OnDestroy {
  resendForm: FormGroup;
  loading = false;
  success = false;
  countdown = 0;
  private destroy$ = new Subject<void>();
  private readonly RESEND_COOLDOWN = 60; // seconds

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthenticationService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.initializeForm();
  }

  private initializeForm() {
    this.resendForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  get f() { return this.resendForm.controls; }

  onSubmit() {
    if (this.resendForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    if (this.countdown > 0) {
      this.notificationService.showWarning(`Please wait ${this.countdown} seconds before requesting another verification email.`);
      return;
    }

    this.loading = true;
    this.success = false;

    this.authService.resendVerificationEmail(this.f.email.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.loading = false;
          this.success = true;
          this.startCountdown();
          this.notificationService.showSuccess('Verification email sent successfully! Please check your inbox.');
        },
        error: (error) => {
          this.loading = false;
          this.success = false;
          const errorMessage = this.getErrorMessage(error);
          this.notificationService.showError(errorMessage);
        }
      });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 404) {
      return 'No account found with this email address.';
    } else if (error.status === 409) {
      return 'This email address has already been verified.';
    } else if (error.status === 429) {
      return 'Too many requests. Please wait before trying again.';
    } else {
      return error.message || 'Failed to send verification email. Please try again.';
    }
  }

  private startCountdown() {
    this.countdown = this.RESEND_COOLDOWN;
    const countdownInterval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(countdownInterval);
      }
    }, 1000);
  }

  private markFormGroupTouched() {
    Object.keys(this.resendForm.controls).forEach(key => {
      this.resendForm.get(key).markAsTouched();
    });
  }

  navigateToLogin() {
    this.router.navigate(['/auth/login']);
  }

  navigateToRegister() {
    this.router.navigate(['/auth/register']);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}