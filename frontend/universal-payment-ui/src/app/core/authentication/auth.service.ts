import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, map, tap, filter, take, switchMap } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';

export interface User {
  id: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
  isVerified: boolean;
  twoFactorEnabled: boolean;
  lastLogin?: Date;
  avatar?: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
  twoFactorCode?: string;
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  requiresTwoFactor?: boolean;
}

export interface TokenRefreshResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface TwoFactorResponse {
  success: boolean;
  requiresTwoFactor?: boolean;
  recoveryCodes?: string[];
}

export interface OAuthResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  isNewUser?: boolean;
}

export interface RegisterCredentials {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
  acceptTerms: boolean;
}

export interface RegisterResponse {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  requiresVerification?: boolean;
  message?: string;
}

// Custom error class for authentication errors
export class AuthError extends Error {
  constructor(
    message: string,
    public code?: string,
    public status?: number
  ) {
    super(message);
    this.name = 'AuthError';
  }
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private tokenRefreshTimeout: any;
  private refreshInProgress: boolean = false;
  private refreshSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);
  
  // Storage keys
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';
  private readonly REMEMBER_ME_KEY = 'remember_me';
  
  // API configuration
  private readonly API_URL = '/api/auth';
  private readonly TOKEN_REFRESH_OFFSET = 300; // 5 minutes before expiry
  
  // Error messages mapping
  private readonly ERROR_MESSAGES: { [key: string]: string } = {
    'invalid_credentials': 'Invalid email or password',
    'account_locked': 'Account temporarily locked. Please try again later or reset your password.',
    'email_not_verified': 'Please verify your email address before logging in.',
    'two_factor_required': 'Two-factor authentication required',
    'invalid_two_factor': 'Invalid verification code',
    'token_expired': 'Session expired, please login again',
    'invalid_token': 'Invalid authentication token',
    'user_not_found': 'User account not found',
    'weak_password': 'Password does not meet security requirements',
    'email_exists': 'An account with this email already exists',
    'rate_limited': 'Too many attempts. Please try again later.',
    'invalid_refresh_token': 'Refresh token is invalid or expired',
    'oauth_failed': 'OAuth authentication failed'
  };

  constructor(
    private router: Router,
    private http: HttpClient,
    private jwtHelper: JwtHelperService,
    @Inject(PLATFORM_ID) private platformId: any
  ) {
    this.currentUserSubject = new BehaviorSubject<User | null>(
      this.getStoredUser()
    );
    this.currentUser = this.currentUserSubject.asObservable();
    
    // Auto-refresh token on service initialization
    if (this.isAuthenticated()) {
      this.scheduleTokenRefresh(this.getTokenExpiration());
    }
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get accessToken(): string | null {
    return this.getStoredToken();
  }

  public get refreshTokenValue(): string | null {
    return this.getStoredRefreshToken();
  }

  // Main login method
  login(credentials: LoginCredentials): Observable<AuthResponse> {
    this.logAuthAction('Login attempt', { email: credentials.email });
    
    return this.http.post<AuthResponse>(`${this.API_URL}/login`, credentials)
      .pipe(
        tap(response => {
          if (!this.validateToken(response.accessToken)) {
            throw new AuthError('Invalid token received from server', 'invalid_token');
          }
          
          if (!response.requiresTwoFactor) {
            this.setAuthData(response, credentials.rememberMe || false);
            this.scheduleTokenRefresh(response.expiresIn);
            this.logAuthAction('Login successful', { userId: response.user.id });
          } else {
            this.logAuthAction('Two-factor authentication required', { userId: response.user?.id });
          }
        }),
        catchError(error => {
          this.logAuthAction('Login failed', { error: error.message });
          return throwError(() => this.handleAuthError(error));
        })
      );
  }

  // Registration method
  register(credentials: RegisterCredentials): Observable<RegisterResponse> {
    this.logAuthAction('Registration attempt', { email: credentials.email });
    
    return this.http.post<RegisterResponse>(`${this.API_URL}/register`, credentials)
      .pipe(
        tap(response => {
          if (!this.validateToken(response.accessToken)) {
            throw new AuthError('Invalid token received from server', 'invalid_token');
          }
          
          if (!response.requiresVerification) {
            this.setAuthData(response, false);
            this.scheduleTokenRefresh(response.expiresIn);
            this.logAuthAction('Registration successful', { userId: response.user.id });
          } else {
            this.logAuthAction('Email verification required', { userId: response.user.id });
          }
        }),
        catchError(error => {
          this.logAuthAction('Registration failed', { error: error.message });
          return throwError(() => this.handleAuthError(error));
        })
      );
  }

  // Two-Factor Authentication methods
  verifyTwoFactorCode(code: string, rememberDevice: boolean = false): Observable<AuthResponse> {
    this.logAuthAction('Two-factor verification attempt');
    
    return this.http.post<AuthResponse>(`${this.API_URL}/verify-2fa`, {
      code,
      rememberDevice
    }).pipe(
      tap(response => {
        if (!this.validateToken(response.accessToken)) {
          throw new AuthError('Invalid token received from server', 'invalid_token');
        }
        
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
        this.logAuthAction('Two-factor verification successful', { userId: response.user.id });
      }),
      catchError(error => {
        this.logAuthAction('Two-factor verification failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  resendTwoFactorCode(): Observable<{ success: boolean }> {
    this.logAuthAction('Resend two-factor code');
    
    return this.http.post<{ success: boolean }>(`${this.API_URL}/resend-2fa`, {})
      .pipe(
        tap(() => this.logAuthAction('Two-factor code resent')),
        catchError(error => {
          this.logAuthAction('Resend two-factor code failed', { error: error.message });
          return throwError(() => this.handleAuthError(error));
        })
      );
  }

  verifyBackupCode(backupCode: string): Observable<AuthResponse> {
    this.logAuthAction('Backup code verification attempt');
    
    return this.http.post<AuthResponse>(`${this.API_URL}/verify-backup-code`, {
      backupCode
    }).pipe(
      tap(response => {
        if (!this.validateToken(response.accessToken)) {
          throw new AuthError('Invalid token received from server', 'invalid_token');
        }
        
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
        this.logAuthAction('Backup code verification successful', { userId: response.user.id });
      }),
      catchError(error => {
        this.logAuthAction('Backup code verification failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Email verification methods
  resendVerificationEmail(email: string): Observable<{ success: boolean; message: string }> {
    this.logAuthAction('Resend verification email', { email });
    
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/resend-verification`,
      { email }
    ).pipe(
      tap(() => this.logAuthAction('Verification email resent')),
      catchError(error => {
        this.logAuthAction('Resend verification email failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  verifyEmail(token: string): Observable<{ success: boolean; user: User }> {
    this.logAuthAction('Email verification attempt');
    
    return this.http.post<{ success: boolean; user: User }>(
      `${this.API_URL}/verify-email`,
      { token }
    ).pipe(
      tap(response => {
        if (response.success && this.currentUserValue) {
          const updatedUser = { ...this.currentUserValue, isVerified: true };
          this.currentUserSubject.next(updatedUser);
          this.updateStoredUser(updatedUser);
          this.logAuthAction('Email verification successful', { userId: updatedUser.id });
        }
      }),
      catchError(error => {
        this.logAuthAction('Email verification failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Account unlock methods
  sendUnlockAccountEmail(email: string): Observable<{ success: boolean; message: string }> {
    this.logAuthAction('Send unlock account email', { email });
    
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/send-unlock-account`,
      { email }
    ).pipe(
      tap(() => this.logAuthAction('Unlock account email sent')),
      catchError(error => {
        this.logAuthAction('Send unlock account email failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  unlockAccount(token: string): Observable<{ success: boolean; message: string }> {
    this.logAuthAction('Unlock account attempt');
    
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/unlock-account`,
      { token }
    ).pipe(
      tap(() => this.logAuthAction('Account unlocked successfully')),
      catchError(error => {
        this.logAuthAction('Unlock account failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // OAuth methods
  exchangeOAuthCode(code: string, provider: string): Observable<OAuthResponse> {
    this.logAuthAction('OAuth code exchange', { provider });
    
    return this.http.post<OAuthResponse>(`${this.API_URL}/oauth/callback`, {
      code,
      provider,
      grant_type: 'authorization_code'
    }).pipe(
      tap(response => {
        if (!this.validateToken(response.accessToken)) {
          throw new AuthError('Invalid token received from server', 'invalid_token');
        }
        
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
        this.logAuthAction('OAuth authentication successful', { 
          userId: response.user.id, 
          provider,
          isNewUser: response.isNewUser 
        });
      }),
      catchError(error => {
        this.logAuthAction('OAuth authentication failed', { provider, error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  authenticateWithOAuthToken(accessToken: string, idToken: string | null, provider: string): Observable<OAuthResponse> {
    this.logAuthAction('OAuth token authentication', { provider });
    
    return this.http.post<OAuthResponse>(`${this.API_URL}/oauth/token`, {
      access_token: accessToken,
      id_token: idToken,
      provider
    }).pipe(
      tap(response => {
        if (!this.validateToken(response.accessToken)) {
          throw new AuthError('Invalid token received from server', 'invalid_token');
        }
        
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
        this.logAuthAction('OAuth token authentication successful', { 
          userId: response.user.id, 
          provider 
        });
      }),
      catchError(error => {
        this.logAuthAction('OAuth token authentication failed', { provider, error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Password reset methods
  forgotPassword(email: string): Observable<{ success: boolean; message: string }> {
    this.logAuthAction('Forgot password request', { email });
    
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/forgot-password`,
      { email }
    ).pipe(
      tap(() => this.logAuthAction('Password reset email sent')),
      catchError(error => {
        this.logAuthAction('Forgot password request failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  resetPassword(token: string, newPassword: string): Observable<{ success: boolean; message: string }> {
    this.logAuthAction('Password reset attempt');
    
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/reset-password`,
      { token, newPassword }
    ).pipe(
      tap(() => this.logAuthAction('Password reset successful')),
      catchError(error => {
        this.logAuthAction('Password reset failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Two-Factor setup methods
  setupTwoFactor(): Observable<{ secret: string; qrCode: string; recoveryCodes: string[] }> {
    this.logAuthAction('Two-factor setup initialization');
    
    return this.http.post<{ secret: string; qrCode: string; recoveryCodes: string[] }>(
      `${this.API_URL}/setup-2fa`,
      {}
    ).pipe(
      tap(() => this.logAuthAction('Two-factor setup initialized')),
      catchError(error => {
        this.logAuthAction('Two-factor setup failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  enableTwoFactor(verificationCode: string): Observable<TwoFactorResponse> {
    this.logAuthAction('Enable two-factor attempt');
    
    return this.http.post<TwoFactorResponse>(
      `${this.API_URL}/enable-2fa`,
      { verificationCode }
    ).pipe(
      tap(response => {
        if (response.success && this.currentUserValue) {
          const updatedUser = { ...this.currentUserValue, twoFactorEnabled: true };
          this.currentUserSubject.next(updatedUser);
          this.updateStoredUser(updatedUser);
          this.logAuthAction('Two-factor enabled successfully', { userId: updatedUser.id });
        }
      }),
      catchError(error => {
        this.logAuthAction('Enable two-factor failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  disableTwoFactor(verificationCode: string): Observable<{ success: boolean }> {
    this.logAuthAction('Disable two-factor attempt');
    
    return this.http.post<{ success: boolean }>(
      `${this.API_URL}/disable-2fa`,
      { verificationCode }
    ).pipe(
      tap(response => {
        if (response.success && this.currentUserValue) {
          const updatedUser = { ...this.currentUserValue, twoFactorEnabled: false };
          this.currentUserSubject.next(updatedUser);
          this.updateStoredUser(updatedUser);
          this.logAuthAction('Two-factor disabled successfully', { userId: updatedUser.id });
        }
      }),
      catchError(error => {
        this.logAuthAction('Disable two-factor failed', { error: error.message });
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Logout
  logout(redirectTo: string = '/auth/login', reason?: string): void {
    this.logAuthAction('Logout', { reason });
    this.clearAuthData();
    this.clearRefreshTimeout();
    this.currentUserSubject.next(null);
    this.refreshInProgress = false;
    this.refreshSubject.next(null);
    this.router.navigate([redirectTo]);
  }

  // Enhanced Token management with race condition protection
  refreshToken(): Observable<TokenRefreshResponse> {
    if (this.refreshInProgress) {
      return this.refreshSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(() => {
          const token = this.getStoredToken();
          const refreshToken = this.refreshTokenValue;
          if (!token || !refreshToken) {
            return throwError(() => new AuthError('No tokens available after refresh', 'no_tokens'));
          }
          return of({ 
            accessToken: token, 
            refreshToken: refreshToken,
            expiresIn: this.getTokenExpiration()
          });
        })
      );
    }

    this.refreshInProgress = true;
    const refreshToken = this.refreshTokenValue;
    
    if (!refreshToken) {
      this.refreshInProgress = false;
      this.logAuthAction('Token refresh failed - no refresh token');
      return throwError(() => new AuthError('No refresh token available', 'no_refresh_token'));
    }

    this.logAuthAction('Token refresh attempt');
    
    return this.http.post<TokenRefreshResponse>(`${this.API_URL}/refresh`, {
      refreshToken
    }).pipe(
      tap(response => {
        if (!this.validateToken(response.accessToken)) {
          throw new AuthError('Invalid token received during refresh', 'invalid_token');
        }
        
        this.setToken(response.accessToken);
        this.setRefreshToken(response.refreshToken);
        this.scheduleTokenRefresh(response.expiresIn);
        this.refreshInProgress = false;
        this.refreshSubject.next(response.accessToken);
        this.logAuthAction('Token refresh successful');
      }),
      catchError(error => {
        this.refreshInProgress = false;
        this.refreshSubject.next(null);
        this.logAuthAction('Token refresh failed', { error: error.message });
        this.logout('/auth/login', 'token_refresh_failed');
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Enhanced Authentication checks
  isAuthenticated(): boolean {
    const token = this.getStoredToken();
    if (!token) return false;

    try {
      const isValid = !this.jwtHelper.isTokenExpired(token) && this.validateToken(token);
      if (!isValid) {
        this.logAuthAction('Token validation failed');
      }
      return isValid;
    } catch {
      return false;
    }
  }

  isVerified(): boolean {
    return this.currentUserValue?.isVerified || false;
  }

  hasRole(role: string | string[]): boolean {
    const user = this.currentUserValue;
    if (!user) return false;

    const roles = Array.isArray(role) ? role : [role];
    return roles.some(r => user.roles.includes(r));
  }

  hasPermission(permission: string | string[]): boolean {
    const user = this.currentUserValue;
    if (!user) return false;

    const permissions = Array.isArray(permission) ? permission : [permission];
    return permissions.some(p => user.permissions.includes(p));
  }

  canAccess(route: any): boolean {
    if (route.data && route.data.roles) {
      return this.hasRole(route.data.roles);
    }

    if (route.data && route.data.permissions) {
      return this.hasPermission(route.data.permissions);
    }

    return true;
  }

  // New utility methods
  getUserRoles(): string[] {
    return this.currentUserValue?.roles || [];
  }

  getUserPermissions(): string[] {
    return this.currentUserValue?.permissions || [];
  }

  isTokenExpiringSoon(minutes: number = 5): boolean {
    const token = this.getStoredToken();
    if (!token) return true;

    try {
      const expiration = this.jwtHelper.getTokenExpirationDate(token);
      if (!expiration) return true;
      
      const now = new Date();
      const timeUntilExpiry = expiration.getTime() - now.getTime();
      return timeUntilExpiry < (minutes * 60 * 1000);
    } catch {
      return true;
    }
  }

  ensureValidToken(): Observable<boolean> {
    if (this.isTokenExpiringSoon()) {
      return this.refreshToken().pipe(
        map(() => true),
        catchError(() => of(false))
      );
    }
    return of(true);
  }

  // Enhanced Storage management with security
  private setAuthData(authResponse: AuthResponse, rememberMe: boolean): void {
    if (isPlatformBrowser(this.platformId)) {
      const storage = rememberMe ? localStorage : sessionStorage;
      
      this.setSecureToken(this.TOKEN_KEY, authResponse.accessToken);
      this.setSecureToken(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
      storage.setItem(this.USER_KEY, JSON.stringify(authResponse.user));
      localStorage.setItem(this.REMEMBER_ME_KEY, rememberMe.toString());
      
      this.currentUserSubject.next(authResponse.user);
    }
  }

  private updateStoredUser(user: User): void {
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      storage.setItem(this.USER_KEY, JSON.stringify(user));
    }
  }

  private setToken(token: string): void {
    this.setSecureToken(this.TOKEN_KEY, token);
  }

  private setRefreshToken(token: string): void {
    this.setSecureToken(this.REFRESH_TOKEN_KEY, token);
  }

  private setSecureToken(key: string, value: string): void {
    if (isPlatformBrowser(this.platformId)) {
      try {
        // Basic encoding for storage - consider crypto-js for production with proper key management
        const encodedValue = btoa(unescape(encodeURIComponent(value)));
        const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
        const storage = rememberMe ? localStorage : sessionStorage;
        storage.setItem(key, encodedValue);
      } catch (e) {
        console.error('Failed to store token securely:', e);
        // Fallback to plain storage if encoding fails
        const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
        const storage = rememberMe ? localStorage : sessionStorage;
        storage.setItem(key, value);
      }
    }
  }

  private getStoredToken(): string | null {
    return this.getSecureToken(this.TOKEN_KEY);
  }

  private getStoredRefreshToken(): string | null {
    return this.getSecureToken(this.REFRESH_TOKEN_KEY);
  }

  private getSecureToken(key: string): string | null {
    if (isPlatformBrowser(this.platformId)) {
      try {
        const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
        const storage = rememberMe ? localStorage : sessionStorage;
        const encodedValue = storage.getItem(key);
        
        if (!encodedValue) return null;
        
        // Try to decode as base64 first, fallback to plain text
        try {
          return decodeURIComponent(escape(atob(encodedValue)));
        } catch {
          // If decoding fails, assume it's stored in plain text (backward compatibility)
          return encodedValue;
        }
      } catch (e) {
        console.error('Failed to retrieve token:', e);
        return null;
      }
    }
    return null;
  }

  private getStoredUser(): User | null {
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      const userStr = storage.getItem(this.USER_KEY);
      return userStr ? JSON.parse(userStr) : null;
    }
    return null;
  }

  private clearAuthData(): void {
    if (isPlatformBrowser(this.platformId)) {
      // Clear both storage locations to ensure complete cleanup
      [localStorage, sessionStorage].forEach(storage => {
        storage.removeItem(this.TOKEN_KEY);
        storage.removeItem(this.REFRESH_TOKEN_KEY);
        storage.removeItem(this.USER_KEY);
      });
      localStorage.removeItem(this.REMEMBER_ME_KEY);
    }
  }

  // Token refresh management
  private clearRefreshTimeout(): void {
    if (this.tokenRefreshTimeout) {
      clearTimeout(this.tokenRefreshTimeout);
      this.tokenRefreshTimeout = null;
    }
  }

  private scheduleTokenRefresh(expiresIn: number): void {
    this.clearRefreshTimeout();
    
    // Refresh token before expiry (default 5 minutes)
    const refreshTime = (expiresIn - this.TOKEN_REFRESH_OFFSET) * 1000;
    
    if (refreshTime > 0) {
      this.tokenRefreshTimeout = setTimeout(() => {
        this.logAuthAction('Scheduled token refresh triggered');
        this.refreshToken().subscribe({
          error: (error) => {
            this.logAuthAction('Scheduled token refresh failed', { error: error.message });
            this.logout();
          }
        });
      }, refreshTime);
    } else {
      this.logAuthAction('Token expiry too short for refresh scheduling', { expiresIn });
    }
  }

  private getTokenExpiration(): number {
    const token = this.getStoredToken();
    if (!token) return 0;

    try {
      const decoded = this.jwtHelper.decodeToken(token);
      return decoded.exp - Math.floor(Date.now() / 1000);
    } catch {
      return 0;
    }
  }

  // Token validation
  private validateToken(token: string): boolean {
    if (!token || typeof token !== 'string') {
      return false;
    }

    try {
      const decoded = this.jwtHelper.decodeToken(token);
      return !!(decoded && 
                decoded.sub && 
                decoded.exp && 
                typeof decoded.exp === 'number' &&
                decoded.exp > Math.floor(Date.now() / 1000));
    } catch {
      return false;
    }
  }

  // Enhanced Error handling
  private handleAuthError(error: any): AuthError {
    // Network or connection errors
    if (error.status === 0) {
      return new AuthError(
        'Unable to connect to authentication server. Please check your internet connection.',
        'network_error',
        error.status
      );
    }

    // Server response with error information
    const serverError = error.error?.error;
    const serverMessage = error.error?.message;
    
    let message = this.ERROR_MESSAGES[serverError] || 
                 serverMessage || 
                 'An unexpected authentication error occurred';

    // HTTP status based errors
    if (error.status === 401) {
      message = this.ERROR_MESSAGES[serverError] || 'Your session has expired. Please login again.';
    } else if (error.status === 403) {
      message = 'You do not have permission to access this resource.';
    } else if (error.status === 429) {
      message = 'Too many requests. Please wait a moment and try again.';
    } else if (error.status >= 500) {
      message = 'Authentication service is temporarily unavailable. Please try again later.';
    }

    return new AuthError(message, serverError, error.status);
  }

  // HTTP interceptor helper
  getAuthHeaders(): HttpHeaders {
    const token = this.getStoredToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
      'X-Requested-With': 'XMLHttpRequest'
    });
  }

  // Enhanced logging for debugging
  private logAuthAction(action: string, data?: any): void {
    // In production, you might want to send these to your logging service
    if (typeof window !== 'undefined' && (window as any).ENV === 'production') {
      return;
    }
    
    console.log(`[AuthService] ${action}`, {
      timestamp: new Date().toISOString(),
      userId: this.currentUserValue?.id,
      isAuthenticated: this.isAuthenticated(),
      ...data
    });
  }

  // Development helpers (remove in production)
  private generateMockToken(user: User): string {
    const payload = {
      sub: user.id,
      username: user.username,
      email: user.email,
      roles: user.roles,
      permissions: user.permissions,
      exp: Math.floor(Date.now() / 1000) + 3600,
      iat: Math.floor(Date.now() / 1000)
    };
    return `mock.${btoa(JSON.stringify(payload))}.signature`;
  }

  private generateMockRefreshToken(user: User): string {
    const payload = {
      sub: user.id,
      type: 'refresh',
      exp: Math.floor(Date.now() / 1000) + 86400
    };
    return `mock.refresh.${btoa(JSON.stringify(payload))}.signature`;
  }

  // Utility method to check storage status (for debugging)
  getStorageStatus(): { hasToken: boolean; hasRefreshToken: boolean; hasUser: boolean; storageType: string } {
    const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
    const storage = rememberMe ? localStorage : sessionStorage;
    
    return {
      hasToken: !!storage.getItem(this.TOKEN_KEY),
      hasRefreshToken: !!storage.getItem(this.REFRESH_TOKEN_KEY),
      hasUser: !!storage.getItem(this.USER_KEY),
      storageType: rememberMe ? 'localStorage' : 'sessionStorage'
    };
  }
}