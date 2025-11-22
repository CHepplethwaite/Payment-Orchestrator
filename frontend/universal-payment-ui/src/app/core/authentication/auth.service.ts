import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
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
  username: string;
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

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private tokenRefreshTimeout: any;
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';
  private readonly REMEMBER_ME_KEY = 'remember_me';
  private readonly API_URL = '/api/auth';

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
    return this.http.post<AuthResponse>(`${this.API_URL}/login`, credentials)
      .pipe(
        tap(response => {
          if (!response.requiresTwoFactor) {
            this.setAuthData(response, credentials.rememberMe || false);
            this.scheduleTokenRefresh(response.expiresIn);
          }
        }),
        catchError(error => {
          return throwError(() => this.handleAuthError(error));
        })
      );
  }

  // Two-Factor Authentication methods
  verifyTwoFactorCode(code: string, rememberDevice: boolean = false): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/verify-2fa`, {
      code,
      rememberDevice
    }).pipe(
      tap(response => {
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  resendTwoFactorCode(): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(`${this.API_URL}/resend-2fa`, {})
      .pipe(
        catchError(error => {
          return throwError(() => this.handleAuthError(error));
        })
      );
  }

  verifyBackupCode(backupCode: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/verify-backup-code`, {
      backupCode
    }).pipe(
      tap(response => {
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Email verification methods
  resendVerificationEmail(email: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/resend-verification`,
      { email }
    ).pipe(
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  verifyEmail(token: string): Observable<{ success: boolean; user: User }> {
    return this.http.post<{ success: boolean; user: User }>(
      `${this.API_URL}/verify-email`,
      { token }
    ).pipe(
      tap(response => {
        if (response.success && this.currentUserValue) {
          // Update current user with verified status
          const updatedUser = { ...this.currentUserValue, isVerified: true };
          this.currentUserSubject.next(updatedUser);
          this.updateStoredUser(updatedUser);
        }
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Account unlock methods
  sendUnlockAccountEmail(email: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/send-unlock-account`,
      { email }
    ).pipe(
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  unlockAccount(token: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/unlock-account`,
      { token }
    ).pipe(
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // OAuth methods
  exchangeOAuthCode(code: string, provider: string): Observable<OAuthResponse> {
    return this.http.post<OAuthResponse>(`${this.API_URL}/oauth/callback`, {
      code,
      provider,
      grant_type: 'authorization_code'
    }).pipe(
      tap(response => {
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  authenticateWithOAuthToken(accessToken: string, idToken: string | null, provider: string): Observable<OAuthResponse> {
    return this.http.post<OAuthResponse>(`${this.API_URL}/oauth/token`, {
      access_token: accessToken,
      id_token: idToken,
      provider
    }).pipe(
      tap(response => {
        this.setAuthData(response, false);
        this.scheduleTokenRefresh(response.expiresIn);
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Password reset methods
  forgotPassword(email: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/forgot-password`,
      { email }
    ).pipe(
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  resetPassword(token: string, newPassword: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.API_URL}/reset-password`,
      { token, newPassword }
    ).pipe(
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Two-Factor setup methods
  setupTwoFactor(): Observable<{ secret: string; qrCode: string; recoveryCodes: string[] }> {
    return this.http.post<{ secret: string; qrCode: string; recoveryCodes: string[] }>(
      `${this.API_URL}/setup-2fa`,
      {}
    ).pipe(
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  enableTwoFactor(verificationCode: string): Observable<TwoFactorResponse> {
    return this.http.post<TwoFactorResponse>(
      `${this.API_URL}/enable-2fa`,
      { verificationCode }
    ).pipe(
      tap(response => {
        if (response.success && this.currentUserValue) {
          // Update current user with 2FA status
          const updatedUser = { ...this.currentUserValue, twoFactorEnabled: true };
          this.currentUserSubject.next(updatedUser);
          this.updateStoredUser(updatedUser);
        }
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  disableTwoFactor(verificationCode: string): Observable<{ success: boolean }> {
    return this.http.post<{ success: boolean }>(
      `${this.API_URL}/disable-2fa`,
      { verificationCode }
    ).pipe(
      tap(response => {
        if (response.success && this.currentUserValue) {
          // Update current user with 2FA status
          const updatedUser = { ...this.currentUserValue, twoFactorEnabled: false };
          this.currentUserSubject.next(updatedUser);
          this.updateStoredUser(updatedUser);
        }
      }),
      catchError(error => {
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Logout
  logout(redirectTo: string = '/auth/login'): void {
    this.clearAuthData();
    this.clearRefreshTimeout();
    this.currentUserSubject.next(null);
    this.router.navigate([redirectTo]);
  }

  // Token management
  refreshToken(): Observable<TokenRefreshResponse> {
    const refreshToken = this.refreshTokenValue;
    
    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<TokenRefreshResponse>(`${this.API_URL}/refresh`, {
      refreshToken
    }).pipe(
      tap(response => {
        this.setToken(response.accessToken);
        this.setRefreshToken(response.refreshToken);
        this.scheduleTokenRefresh(response.expiresIn);
      }),
      catchError(error => {
        this.logout();
        return throwError(() => this.handleAuthError(error));
      })
    );
  }

  // Authentication checks
  isAuthenticated(): boolean {
    const token = this.getStoredToken();
    if (!token) return false;

    try {
      return !this.jwtHelper.isTokenExpired(token);
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

  // Storage management
  private setAuthData(authResponse: AuthResponse, rememberMe: boolean): void {
    if (isPlatformBrowser(this.platformId)) {
      const storage = rememberMe ? localStorage : sessionStorage;
      
      storage.setItem(this.TOKEN_KEY, authResponse.accessToken);
      storage.setItem(this.REFRESH_TOKEN_KEY, authResponse.refreshToken);
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
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      storage.setItem(this.TOKEN_KEY, token);
    }
  }

  private setRefreshToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      storage.setItem(this.REFRESH_TOKEN_KEY, token);
    }
  }

  private getStoredToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      return storage.getItem(this.TOKEN_KEY);
    }
    return null;
  }

  private getStoredRefreshToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      return storage.getItem(this.REFRESH_TOKEN_KEY);
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
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
      sessionStorage.removeItem(this.TOKEN_KEY);
      sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
      sessionStorage.removeItem(this.USER_KEY);
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
    
    // Refresh token 5 minutes before expiry
    const refreshTime = (expiresIn - 300) * 1000;
    
    if (refreshTime > 0) {
      this.tokenRefreshTimeout = setTimeout(() => {
        this.refreshToken().subscribe({
          error: () => this.logout()
        });
      }, refreshTime);
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

  // Error handling
  private handleAuthError(error: any): Error {
    if (error.error && error.error.message) {
      return new Error(error.error.message);
    }
    
    if (error.status === 0) {
      return new Error('Unable to connect to authentication server');
    }
    
    if (error.status === 401) {
      return new Error('Authentication failed');
    }
    
    if (error.status === 403) {
      return new Error('Access denied');
    }
    
    if (error.status === 429) {
      return new Error('Too many requests. Please try again later.');
    }
    
    return new Error(error.message || 'An unexpected error occurred');
  }

  // HTTP interceptor helper
  getAuthHeaders(): HttpHeaders {
    const token = this.getStoredToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  // Mock methods for development (remove in production)
  private generateMockToken(user: User): string {
    // In real implementation, this would be a JWT from the server
    const payload = {
      sub: user.id,
      username: user.username,
      email: user.email,
      roles: user.roles,
      permissions: user.permissions,
      exp: Math.floor(Date.now() / 1000) + 3600 // 1 hour
    };
    return `mock.${btoa(JSON.stringify(payload))}.signature`;
  }

  private generateMockRefreshToken(user: User): string {
    const payload = {
      sub: user.id,
      type: 'refresh',
      exp: Math.floor(Date.now() / 1000) + 86400 // 24 hours
    };
    return `mock.refresh.${btoa(JSON.stringify(payload))}.signature`;
  }
}