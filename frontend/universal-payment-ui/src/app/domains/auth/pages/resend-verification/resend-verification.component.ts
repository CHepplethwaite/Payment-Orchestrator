import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../core/services/notification/notification.service';

@Component({
  selector: 'app-resend-verification',
  templateUrl: './resend-verification.component.html',
  styleUrls: ['./resend-verification.component.css'], // or .css if using CSS
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule] // Add required imports
})
export class ResendVerificationComponent implements OnInit, OnDestroy {
  resendForm!: FormGroup;
  loading = false;
  success = false;
  countdown = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.resendForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  // Safe getter for form controls with bracket notation
  get f(): { [key: string]: AbstractControl } {
    return this.resendForm.controls;
  }

  onSubmit() {
    if (this.resendForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    const email = this.resendForm.get('email')?.value;

    if (!email) {
      this.loading = false;
      return;
    }

    this.authService.resendVerificationEmail(email)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          this.loading = false;
          this.success = true;
          this.startCountdown(60); // 60 seconds countdown
          this.notificationService.showSuccess('Verification email sent successfully!');
        },
        error: (error: Error) => {
          this.loading = false;
          this.notificationService.showError(error.message || 'Failed to send verification email');
        }
      });
  }

  private markFormGroupTouched() {
    if (this.resendForm) {
      this.resendForm.markAllAsTouched();
    }
  }

  startCountdown(seconds: number) {
    this.countdown = seconds;
    const interval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(interval);
      }
    }, 1000);
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