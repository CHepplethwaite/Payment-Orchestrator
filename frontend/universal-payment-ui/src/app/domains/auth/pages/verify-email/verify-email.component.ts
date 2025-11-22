import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../core/services/notification/notification.service';

@Component({
  selector: 'app-verify-email',
  templateUrl: './verify-email.component.html',
  styleUrls: ['./verify-email.component.css']
})
export class VerifyEmailComponent implements OnInit, OnDestroy {
  loading = true;
  verified = false;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    const token = this.route.snapshot.queryParams['token'];
    
    if (!token) {
      this.notificationService.showError('Invalid verification link');
      this.router.navigate(['/auth/login']);
      return;
    }

    this.verifyEmail(token);
  }

  verifyEmail(token: string) {
    this.authService.verifyEmail(token)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.verified = true;
          this.loading = false;
          this.notificationService.showSuccess('Email verified successfully!');
        },
        error: (error) => {
          this.verified = false;
          this.loading = false;
          this.notificationService.showError(error.message || 'Email verification failed');
        }
      });
  }

  navigateToLogin() {
    this.router.navigate(['/auth/login']);
  }

  resendVerification() {
    this.router.navigate(['/auth/resend-verification']);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}