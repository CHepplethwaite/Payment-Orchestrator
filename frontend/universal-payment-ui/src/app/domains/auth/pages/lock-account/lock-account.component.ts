import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../core/services/notification/notification.service';

@Component({
  selector: 'app-lock-account',
  templateUrl: './lock-account.component.html',
  styleUrls: ['./lock-account.component.scss'],
  standalone: true,
  imports: [
    CommonModule,        // Required for *ngIf, *ngFor, etc.
    ReactiveFormsModule, // Required for formGroup, formControlName
    RouterModule         // Required for routerLink
  ]
})
export class LockAccountComponent implements OnInit, OnDestroy {
  unlockForm!: FormGroup;
  loading = false;
  success = false;
  countdown = 0;
  isAutoLocked = true;
  
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.unlockForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit() {
    if (!this.unlockForm || this.unlockForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    const email = this.unlockForm.get('email')?.value;
    
    if (!email) {
      this.loading = false;
      return;
    }

    // Use your AuthService method for unlock account
    this.authService.sendUnlockAccountEmail(email)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.success = true;
          this.loading = false;
          this.startCountdown(60); // 60 seconds countdown
          this.notificationService.showSuccess('Unlock instructions sent to your email');
        },
        error: (error) => {
          this.loading = false;
          this.notificationService.showError(error.message || 'Failed to send unlock instructions');
        }
      });
  }

  private markFormGroupTouched() {
    if (this.unlockForm) {
      this.unlockForm.markAllAsTouched();
    }
  }

  attemptLogin() {
    this.router.navigate(['/auth/login']);
  }

  navigateToSupport() {
    // Implement support navigation
    console.log('Navigate to support');
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

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}