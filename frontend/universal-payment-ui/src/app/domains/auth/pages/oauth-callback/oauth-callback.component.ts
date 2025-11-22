import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common'; // Add this import
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../core/services/notification/notification.service';

@Component({
  selector: 'app-oauth-callback',
  template: `
    <div class="oauth-callback-container">
      <div class="card">
        <div class="card-body text-center">
          <div *ngIf="processing" class="processing-state">
            <div class="spinner-border text-primary" role="status"></div>
            <h3>Completing Authentication...</h3>
            <p>Please wait while we sign you in.</p>
          </div>

          <div *ngIf="success" class="success-state">
            <div class="success-icon">✓</div>
            <h3>Authentication Successful!</h3>
            <p>Redirecting you to your dashboard...</p>
          </div>

          <div *ngIf="error" class="error-state">
            <div class="error-icon">⚠️</div>
            <h3>Authentication Failed</h3>
            <p>{{ errorMessage }}</p>
            <div class="error-actions">
              <button class="btn btn-primary" (click)="retryOAuth()">Try Again</button>
              <button class="btn btn-outline-secondary" (click)="navigateToLogin()">Back to Login</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./oauth-callback.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule] // Add CommonModule here
})
export class OauthCallbackComponent implements OnInit, OnDestroy {
  processing = true;
  success = false;
  error = false;
  errorMessage = '';
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.processOAuthCallback();
  }

  private processOAuthCallback() {
    // Get all possible OAuth parameters from the URL
    const queryParams = this.route.snapshot.queryParams;
    const fragment = this.route.snapshot.fragment;

    // Extract parameters from either query params or fragment (depending on OAuth provider)
    const params = this.extractOAuthParams(queryParams, fragment);

    // Check for OAuth errors first
    if (params.error) {
      this.handleOAuthError(params);
      return;
    }

    // Process successful OAuth response
    this.handleOAuthSuccess(params);
  }

  private extractOAuthParams(queryParams: any, fragment: string | null): any {
    const params: any = {};

    // Check for error in query params
    if (queryParams.error) {
      params.error = queryParams.error;
      params.error_description = queryParams.error_description;
      return params;
    }

    // Some OAuth providers use fragments, others use query parameters
    if (fragment) {
      const fragmentParams = new URLSearchParams(fragment);
      params.access_token = fragmentParams.get('access_token');
      params.token_type = fragmentParams.get('token_type');
      params.expires_in = fragmentParams.get('expires_in');
      params.state = fragmentParams.get('state');
    }

    // Check query parameters for common OAuth response fields
    if (queryParams.code) {
      params.code = queryParams.code;
    }
    if (queryParams.state) {
      params.state = queryParams.state;
    }
    if (queryParams.access_token) {
      params.access_token = queryParams.access_token;
    }
    if (queryParams.id_token) {
      params.id_token = queryParams.id_token;
    }

    return params;
  }

  private handleOAuthSuccess(params: any) {
    const provider = this.getProviderFromState(params.state);
    const returnUrl = this.getReturnUrlFromState(params.state);

    if (params.code) {
      // Authorization code flow - exchange code for tokens
      this.exchangeCodeForToken(params.code, provider, returnUrl);
    } else if (params.access_token) {
      // Implicit flow - use the access token directly
      this.authenticateWithToken(params.access_token, params.id_token, provider, returnUrl);
    } else {
      this.handleError('Invalid OAuth response. Missing authentication data.');
    }
  }

  private exchangeCodeForToken(code: string, provider: string, returnUrl: string) {
    this.authService.exchangeOAuthCode(code, provider)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.handleAuthenticationSuccess(response, returnUrl);
        },
        error: (error) => {
          this.handleError(this.getExchangeErrorMessage(error, provider));
        }
      });
  }

  private authenticateWithToken(accessToken: string, idToken: string | null, provider: string, returnUrl: string) {
    this.authService.authenticateWithOAuthToken(accessToken, idToken, provider)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.handleAuthenticationSuccess(response, returnUrl);
        },
        error: (error) => {
          this.handleError(this.getTokenErrorMessage(error, provider));
        }
      });
  }

  private handleAuthenticationSuccess(response: any, returnUrl: string) {
    this.processing = false;
    this.success = true;

    // Show success notification
    this.notificationService.showSuccess(`Successfully signed in with ${response.provider || 'OAuth'}!`);

    // Redirect after a brief delay to show success message
    setTimeout(() => {
      this.router.navigate([returnUrl || '/dashboard']);
    }, 1500);
  }

  private handleOAuthError(params: any) {
    let errorMessage = 'Authentication failed.';

    switch (params.error) {
      case 'access_denied':
        errorMessage = 'Authentication was cancelled.';
        break;
      case 'temporarily_unavailable':
        errorMessage = 'Authentication service is temporarily unavailable. Please try again later.';
        break;
      case 'server_error':
        errorMessage = 'Authentication server error. Please try again later.';
        break;
      case 'invalid_request':
        errorMessage = 'Invalid authentication request.';
        break;
      case 'unauthorized_client':
        errorMessage = 'This application is not authorized to use OAuth.';
        break;
      case 'unsupported_response_type':
        errorMessage = 'Unsupported response type.';
        break;
      case 'invalid_scope':
        errorMessage = 'Invalid authentication scope.';
        break;
      default:
        if (params.error_description) {
          errorMessage = params.error_description;
        } else {
          errorMessage = `Authentication error: ${params.error}`;
        }
    }

    this.handleError(errorMessage);
  }

  private handleError(message: string) {
    this.processing = false;
    this.error = true;
    this.errorMessage = message;
    
    this.notificationService.showError(message);
  }

  private getProviderFromState(state: string | null): string {
    if (!state) return 'unknown';
    
    try {
      const stateObj = JSON.parse(atob(state));
      return stateObj.provider || 'unknown';
    } catch {
      return 'unknown';
    }
  }

  private getReturnUrlFromState(state: string | null): string {
    if (!state) return '/dashboard';
    
    try {
      const stateObj = JSON.parse(atob(state));
      return stateObj.returnUrl || '/dashboard';
    } catch {
      return '/dashboard';
    }
  }

  private getExchangeErrorMessage(error: any, provider: string): string {
    if (error.status === 400) {
      return 'Invalid authorization code. Please try signing in again.';
    } else if (error.status === 401) {
      return 'Authentication failed. Please try again.';
    } else if (error.status === 403) {
      return 'This account is not authorized to access the application.';
    } else if (error.status === 409) {
      return 'An account with this email already exists. Please use a different login method.';
    } else if (error.status >= 500) {
      return `${provider} authentication service is temporarily unavailable. Please try again later.`;
    } else {
      return error.message || `Failed to authenticate with ${provider}. Please try again.`;
    }
  }

  private getTokenErrorMessage(error: any, provider: string): string {
    if (error.status === 401) {
      return 'Invalid access token. Please try signing in again.';
    } else if (error.status === 403) {
      return 'This account is not authorized to access the application.';
    } else if (error.status === 409) {
      return 'An account with this email already exists. Please use a different login method.';
    } else {
      return error.message || `Failed to authenticate with ${provider}. Please try again.`;
    }
  }

  retryOAuth() {
    this.error = false;
    this.processing = true;
    
    // Navigate back to login page with OAuth option
    setTimeout(() => {
      this.router.navigate(['/auth/login']);
    }, 500);
  }

  navigateToLogin() {
    this.router.navigate(['/auth/login']);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}