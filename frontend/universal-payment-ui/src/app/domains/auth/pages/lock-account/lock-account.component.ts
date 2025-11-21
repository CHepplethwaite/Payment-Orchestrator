import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthenticationService } from '../../../core/services/authentication.service';
import { NotificationService } from '../../../../shared/services/notification.service';

@Component({
  selector: 'app-lock-account',
  templateUrl: './lock-account.component.html',
  styleUrls: ['./lock-account.component.scss']
})
export class LockAccountComponent implements OnInit, OnDestroy {
  unlockForm: FormGroup;
  loading = false;
  success = false;
  countdown = 0;
  email: string;
  isAutoLocked: boolean = false;
  private destroy$ = new Subject<void>();
  private readonly RESEND_COOLDOWN = 60; // seconds

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthenticationService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.checkRouteParams();
  }

  private initializeForm() {
    this.unlockForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  private checkRouteParams() {
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.email = params['email'];
        this.unlockForm.patchValue({ email: this.email });
      }
      if (params['autoLocked'] === 'true') {
        this.isAutoLocked = true;
      }
    });
  }

  get f() { return this.unlockForm.controls; }

  onSubmit() {
    if (this.unlockForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    if (this.countdown > 0) {
      this.notificationService.showWarning(`Please wait ${this.countdown} seconds before requesting another unlock link.`);
      return;
    }

    this.loading = true;
    this.success = false;

    this.authService.sendUnlockAccountEmail(this.f.email.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.loading = false;
          this.success = true;
          this.startCountdown();
          this.notificationService.showSuccess('Account unlock instructions sent to your email!');
        },
        error: (error) => {
          this.loading = false;
          const errorMessage = this.getErrorMessage(error);
          this.notificationService.showError(errorMessage);
        }
      });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 404) {
      return 'No account found with this email address.';
    } else if (error.status === 403) {
      return 'This account is not locked. You can try logging in.';
    } else if (error.status === 429) {
      return 'Too many unlock requests. Please wait before trying again.';
    } else {
      return error.message || 'Failed to send unlock instructions. Please try again.';
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
    Object.keys(this.unlockForm.controls).forEach(key => {
      this.unlockForm.get(key).markAsTouched();
    });
  }

  attemptLogin() {
    const email = this.f.email.value;
    if (email) {
      this.router.navigate(['/auth/login'], { queryParams: { email } });
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  navigateToSupport() {
    this.router.navigate(['/support']);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}