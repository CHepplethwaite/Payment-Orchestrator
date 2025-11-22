import { Component, OnInit, OnDestroy, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../core/services/notification/notification.service';

@Component({
  selector: 'app-two-factor-auth',
  templateUrl: './two-factor-auth.component.html',
  styleUrls: ['./two-factor-auth.component.css'],
  standalone: true, // Add this if using standalone components
  imports: [CommonModule, ReactiveFormsModule] // Add necessary imports
})
export class TwoFactorAuthComponent implements OnInit, ngOnDestroy, AfterViewInit {

  @ViewChildren('codeInput') codeInputs!: QueryList<ElementRef<HTMLInputElement>>;

  twoFactorForm!: FormGroup;
  loading = false;
  resending = false;
  countdown = 0;
  returnUrl: string = '/dashboard';
  authMethod: 'totp' | 'sms' | 'email' = 'totp';

  private destroy$ = new Subject<void>();
  private countdownInterval: any;
  private readonly RESEND_COOLDOWN = 30;

  // Add this computed property for template
  get showFormError(): boolean {
    return this.codeArray.some(key => {
      const control = this.twoFactorForm.get(key);
      return control ? control.invalid && control.touched : false;
    });
  }

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.initializeForm();
    this.getRouteParams();
  }

  ngAfterViewInit() {
    this.codeInputs.changes
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.focusFirstEmptyInput());

    setTimeout(() => this.focusFirstEmptyInput(), 150);
  }

  // Initialize form
  private initializeForm(): void {
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

  // Route params
  private getRouteParams(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    this.authMethod = this.route.snapshot.queryParams['method'] || 'totp';
  }

  // Focus the first empty input field
  private focusFirstEmptyInput(): void {
    if (!this.codeInputs || this.codeInputs.length === 0) return;

    const inputs = this.codeInputs.toArray();

    for (let i = 0; i < inputs.length; i++) {
      const control = this.twoFactorForm.get(`code${i + 1}`);
      if (control && !control.value) {
        inputs[i].nativeElement.focus();
        return;
      }
    }
  }

  // Code field list
  get codeArray(): string[] {
    return ['code1', 'code2', 'code3', 'code4', 'code5', 'code6'];
  }

  get f() {
    return this.twoFactorForm.controls;
  }

  // Handle input typing
  onInputChange(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    if (value && index < 5) {
      const next = this.codeInputs.toArray()[index + 1];
      next?.nativeElement.focus();
    }

    if (this.isFormComplete() && index === 5) {
      this.onSubmit();
    }
  }

  // Handle backspace + paste
  onKeyDown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Backspace') {
      const current = this.twoFactorForm.get(`code${index + 1}`);

      if (!current?.value && index > 0) {
        const prev = this.codeInputs.toArray()[index - 1];
        prev?.nativeElement.focus();
      }
    }

    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'v') {
      event.preventDefault();
      navigator.clipboard.readText().then(text => this.handlePaste(text));
    }
  }

  // Handle paste (auto fill all 6 digits)
  private handlePaste(text: string): void {
    const clean = text.replace(/\D/g, '');

    if (clean.length === 6) {
      clean.split('').forEach((char, i) => {
        this.twoFactorForm.get(`code${i + 1}`)?.setValue(char);
      });
      this.onSubmit();
    }
  }

  isFormComplete(): boolean {
    return this.codeArray.every(code => this.twoFactorForm.get(code)?.valid);
  }

  getVerificationCode(): string {
    return this.codeArray
      .map(code => this.twoFactorForm.get(code)?.value)
      .join('');
  }

  // Submit verification code
  onSubmit(): void {
    if (!this.isFormComplete()) {
      this.markFormGroupTouched();
      this.notificationService.showError('Please enter the complete 6-digit code');
      return;
    }

    this.loading = true;
    const code = this.getVerificationCode();

    this.authService.verifyTwoFactorCode(code, this.f['rememberDevice'].value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loading = false;
          this.notificationService.showSuccess('Two-factor authentication successful!');
          this.router.navigate([this.returnUrl]);
        },
        error: err => {
          this.loading = false;
          this.clearForm();
          this.notificationService.showError(this.getErrorMessage(err));
          this.focusFirstEmptyInput();
        }
      });
  }

  private getErrorMessage(error: any): string {
    if (error.status === 400) return 'Invalid verification code.';
    if (error.status === 401) return 'Verification code expired.';
    if (error.status === 429) return 'Too many attempts. Please wait.';
    return error.message || 'Verification failed. Please try again.';
  }

  // Resend code
  resendCode(): void {
    if (this.countdown > 0) return;

    this.resending = true;

    this.authService.resendTwoFactorCode()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.resending = false;
          this.startCountdown();
          this.clearForm();
          this.focusFirstEmptyInput();
          this.notificationService.showSuccess('New verification code sent!');
        },
        error: (err: any) => {
          this.resending = false;
          this.notificationService.showError(err.message || 'Failed to resend code.');
        }
      });
  }

  private startCountdown(): void {
    this.countdown = this.RESEND_COOLDOWN;

    clearInterval(this.countdownInterval);
    this.countdownInterval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) clearInterval(this.countdownInterval);
    }, 1000);
  }

  private clearForm(): void {
    this.codeArray.forEach(code => {
      const ctrl = this.twoFactorForm.get(code);
      ctrl?.setValue('');
      ctrl?.markAsUntouched();
    });
  }

  private markFormGroupTouched(): void {
    this.codeArray.forEach(code => {
      this.twoFactorForm.get(code)?.markAsTouched();
    });
  }

  useBackupCode(): void {
    this.router.navigate(['/auth/two-factor-backup']);
  }

  tryAnotherMethod(): void {
    this.router.navigate(['/auth/login']);
  }

  getAuthMethodDisplay(): string {
    switch (this.authMethod) {
      case 'sms': return 'SMS';
      case 'email': return 'Email';
      case 'totp': 
      default: 
        return 'Authenticator App';
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    clearInterval(this.countdownInterval);
  }
}