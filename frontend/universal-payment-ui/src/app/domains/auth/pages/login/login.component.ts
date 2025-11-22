import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../../core/authentication/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule]   // Required for ngModel
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  onSubmit() {
    if (!this.email || !this.password) {
      this.errorMessage = 'Email and password are required.';
      return;
    }

    this.auth.login({
      email: this.email,          // corrected: backend expects email
      password: this.password
    })
    .subscribe({
      next: (res: any) => {
        // If backend indicates 2FA is needed:
        if (res?.requiresTwoFactor) {
          this.router.navigate(
            ['/auth/two-factor'], 
            { queryParams: { method: res.method || 'totp' } }
          );
          return;
        }

        // Otherwise login success
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.errorMessage = 'Invalid email or password';
        console.error('Login error:', err);
      }
    });
  }
}
