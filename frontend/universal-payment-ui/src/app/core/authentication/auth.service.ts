import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
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
}

export interface TokenRefreshResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
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

  constructor(
    private router: Router,
    private jwtHelper: JwtHelperService,
    @Inject(PLATFORM_ID) private platformId: any
  ) {
    this.currentUserSubject = new BehaviorSubject<User | null>(
      this.getStoredUser()
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get accessToken(): string | null {
    return this.getStoredToken();
  }

  public get refreshToken(): string | null {
    return this.getStoredRefreshToken();
  }

  login(credentials: LoginCredentials): Observable<AuthResponse> {
    // Mock login - replace with actual API call
    return new Observable(observer => {
      setTimeout(() => {
        if (credentials.username && credentials.password) {
          const user: User = {
            id: 1,
            username: credentials.username,
            email: `${credentials.username}@mobilemoney.com`,
            firstName: 'System',
            lastName: 'Administrator',
            roles: ['admin'],
            permissions: ['read', 'write', 'delete'],
            isVerified: true,
            twoFactorEnabled: false,
            lastLogin: new Date(),
            avatar: 'assets/images/avatars/admin.png'
          };

          const authResponse: AuthResponse = {
            user,
            accessToken: this.generateMockToken(user),
            refreshToken: this.generateMockRefreshToken(user),
            expiresIn: 3600
          };

          this.setAuthData(authResponse, credentials.rememberMe || false);
          this.scheduleTokenRefresh(authResponse.expiresIn);

          observer.next(authResponse);
          observer.complete();
        } else {
          observer.error(new Error('Invalid credentials'));
        }
      }, 1500);
    });
  }

  logout(redirectTo: string = '/auth/login'): void {
    this.clearAuthData();
    this.clearRefreshTimeout();
    this.currentUserSubject.next(null);
    this.router.navigate([redirectTo]);
  }

  refreshToken(): Observable<TokenRefreshResponse> {
    const refreshToken = this.getStoredRefreshToken();
    
    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    // Mock token refresh - replace with actual API call
    return new Observable(observer => {
      setTimeout(() => {
        const user = this.currentUserValue;
        if (user) {
          const response: TokenRefreshResponse = {
            accessToken: this.generateMockToken(user),
            refreshToken: this.generateMockRefreshToken(user),
            expiresIn: 3600
          };

          this.setToken(response.accessToken);
          this.scheduleTokenRefresh(response.expiresIn);

          observer.next(response);
          observer.complete();
        } else {
          observer.error(new Error('User not authenticated'));
        }
      }, 1000);
    });
  }

  isAuthenticated(): boolean {
    const token = this.getStoredToken();
    if (!token) return false;

    try {
      return !this.jwtHelper.isTokenExpired(token);
    } catch {
      return false;
    }
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

  private setToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      const rememberMe = localStorage.getItem(this.REMEMBER_ME_KEY) === 'true';
      const storage = rememberMe ? localStorage : sessionStorage;
      storage.setItem(this.TOKEN_KEY, token);
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
    
    this.tokenRefreshTimeout = setTimeout(() => {
      this.refreshToken().subscribe({
        error: () => this.logout()
      });
    }, refreshTime);
  }

  private generateMockToken(user: User): string {
    // In real implementation, this would be a JWT from the server
    const payload = {
      sub: user.id,
      username: user.username,
      roles: user.roles,
      permissions: user.permissions,
      exp: Math.floor(Date.now() / 1000) + 3600 // 1 hour
    };
    return btoa(JSON.stringify(payload));
  }

  private generateMockRefreshToken(user: User): string {
    const payload = {
      sub: user.id,
      type: 'refresh',
      exp: Math.floor(Date.now() / 1000) + 86400 // 24 hours
    };
    return btoa(JSON.stringify(payload));
  }
}