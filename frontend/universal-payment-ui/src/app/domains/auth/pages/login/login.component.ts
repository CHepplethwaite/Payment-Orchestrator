import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MockAuthService, LoginResponse } from '../../../../core/authentication/mock-auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  loading = false;

  constructor(private auth: MockAuthService, private router: Router) {}

  onSubmit() {
    this.errorMessage = '';
    this.loading = true;

    console.log('Attempting login with:', this.email, this.password);

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: (res: LoginResponse) => {
        this.loading = false;

        if (res.requiresTwoFactor) {
          // Redirect to 2FA page or show 2FA input
          console.log('2FA required, method:', res.method);
          this.router.navigate(['/auth/2fa']);
        } else if (res.user) {
          // Successful login
          console.log('Login successful for:', res.user.email);
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.message || 'Invalid email or password';
        console.error('Login error:', err);
      }
    });
  }
}
