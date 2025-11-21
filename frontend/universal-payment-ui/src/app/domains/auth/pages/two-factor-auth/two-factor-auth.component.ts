import { Component, OnInit, OnDestroy, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthenticationService } from '../../../core/services/authentication.service';
import { NotificationService } from '../../../../shared/services/notification.service';

@Component({
  selector: 'app-two-factor-auth',
  templateUrl: './two-factor-auth.component.html',
  styleUrls: ['./two-factor-auth.component.scss']
})
export class TwoFactorAuthComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChildren('codeInput') codeInputs!: QueryList<ElementRef>;

  twoFactorForm: FormGroup;
  loading = false;
  resending = false;
  countdown = 0;
  returnUrl: string;
  authMethod: 'totp' | 'sms' | 'email' = 'totp';
  private destroy$ = new Subject<void>();
  private readonly RESEND_COOLDOWN = 30; // seconds

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthenticationService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.getRouteParams();
  }

  ngAfterViewInit() {
    this.setupInputFocus();
  }

  private initializeForm() {
    this.twoFactorForm = this.formBuilder.group({
      code1: ['', [Validators.required, Validators.pattern('[0-9]')]],
      code2: ['', [Validators.required, Validators.pattern('[0-9]')]],
      code3: ['', [Validators.required, Validators.pattern('[0-9]')]],
      code4: ['', [Validators.required, Validators.pattern('[0-9]')]],
      code5: ['', [Validators.required, Validators.pattern('[0-9]')]],
      code6: ['', [Validators.required, Validators.pattern('[0-9]')]],
      rememberDevice: [false]
    });
  }

  private getRouteParams() {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    this.authMethod = this.route.snapshot.queryParams['method'] || 'totp';
  }

  private setupInputFocus() {
    this.codeInputs.changes.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.focusFirstEmptyInput();
    });

    setTimeout(() => this.focusFirstEmptyInput(), 100);
  }

  private focusFirstEmptyInput() {
    const inputs = this.codeInputs.toArray();
    for (let i = 0; i < inputs.length; i++) {
      const control = this.twoFactorForm.get(`code${i + 1}`);
      if (control && !control.value) {
        inputs[i].nativeElement.focus();
        break;
      }
    }
  }

  get codeArray(): string[] {
    return ['code1', 'code2', 'code3', 'code4', 'code5', 'code6'];
  }

  get f() { return this.twoFactorForm.controls; }

  onInputChange(event: any, index: number) {
    const input = event.target;
    const value = input.value;

    // Auto-tab to next input
    if (value && index < 6) {
      const nextInput = this.codeInputs.toArray()[index]?.nativeElement;
      if (nextInput) {
        nextInput.focus();
      }
    }

    // Auto-submit when all fields are filled
    if (this.isFormComplete() && index === 5) {
      this.onSubmit();
    }
  }

  onKeyDown(event: KeyboardEvent, index: number) {
    // Handle backspace
    if (event.key === 'Backspace') {
      const currentControl = this.twoFactorForm.get(`code${index + 1}`);
      if (!currentControl?.value && index > 0) {
        const prevInput = this.codeInputs.toArray()[index - 1]?.nativeElement;
        if (prevInput) {
          prevInput.focus();
        }
      }
    }

    // Handle paste
    if (event.key === 'v' && (event.ctrlKey || event.metaKey)) {
      event.preventDefault();
      navigator.clipboard.readText().then(text => {
        this.handlePaste(text);
      }).catch(() => {
        // Fallback if clipboard API is not available
      });
    }
  }

  private handlePaste(pastedText: string) {
    const cleanCode = pastedText.replace(/\D/g, ''); // Remove non-digits
    if (cleanCode.length === 6) {
      for (let i = 0; i < 6; i++) {
        const control = this.twoFactorForm.get(`code${i + 1}`);
        control?.setValue(cleanCode[i]);
        control?.markAsTouched();
      }
      this.onSubmit();
    }
  }

  private isFormComplete(): boolean {
    return this.codeArray.every(key => this.twoFactorForm.get(key)?.valid);
  }

  getVerificationCode(): string {
    return this.codeArray.map(key => this.twoFactorForm.get(key)?.value).join('');
  }

  onSubmit() {
    if (!this.isFormComplete()) {
      this.markFormGroupTouched();
      this.notificationService.showError('Please enter the complete 6-digit code');
      return;
    }

    this.loading = true;
    const verificationCode = this.getVerificationCode();

    this.authService.verifyTwoFactorCode(verificationCode, this.f.rememberDevice.value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.loading = false;
          this.notificationService.showSuccess('Two-factor authentication successful!');
          this.router.navigate([this.returnUrl]);
        },
        error: (error) => {
          this.loading = false;
          this.clearForm();
          const errorMessage = this.getErrorMessage(error);
          this.notificationService.showError(errorMessage);
          this.focusFirstEmptyInput();
        }
      });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 400) {
      return 'Invalid verification code. Please try again.';
    } else if (error.status === 401) {
      return 'Verification code has expired. Please request a new code.';
    } else if (error.status === 429) {
      return 'Too many failed attempts. Please wait before trying again.';
    } else {
      return error.message || 'Verification failed. Please try again.';
    }
  }

  resendCode() {
    if (this.countdown > 0) {
      this.notificationService.showWarning(`Please wait ${this.countdown} seconds before requesting a new code.`);
      return;
    }

    this.resending = true;
    this.authService.resendTwoFactorCode()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.resending = false;
          this.startCountdown();
          this.clearForm();
          this.focusFirstEmptyInput();
          this.notificationService.showSuccess('New verification code sent!');
        },
        error: (error) => {
          this.resending = false;
          this.notificationService.showError(error.message || 'Failed to send new code. Please try again.');
        }
      });
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

  private clearForm() {
    this.codeArray.forEach(key => {
      this.twoFactorForm.get(key)?.setValue('');
      this.twoFactorForm.get(key)?.markAsUntouched();
    });
  }

  private markFormGroupTouched() {
    this.codeArray.forEach(key => {
      this.twoFactorForm.get(key)?.markAsTouched();
    });
  }

  useBackupCode() {
    this.router.navigate(['/auth/two-factor-backup']);
  }

  tryAnotherMethod() {
    this.router.navigate(['/auth/login']);
  }

  getAuthMethodDisplay(): string {
    switch (this.authMethod) {
      case 'sms': return 'SMS';
      case 'email': return 'Email';
      case 'totp': return 'Authenticator App';
      default: return 'Authenticator App';
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}