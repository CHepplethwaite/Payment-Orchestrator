import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, tap, switchMap, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { StorageService } from '../services/storage.service';
import { EventBusService } from '../services/event-bus.service';
import { LoggerService } from '../services/logger.service';
import { API_CONFIG, ApiConfig } from '../services/api.service';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  permissions: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
  expiresIn: number;
}

export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly STORAGE_KEYS = {
    ACCESS_TOKEN: 'access_token',
    REFRESH_TOKEN: 'refresh_token',
    USER: 'user',
    TOKEN_EXPIRY: 'token_expiry'
  };

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  private refreshTokenInProgress = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(
    private http: HttpClient,
    private storage: StorageService,
    private eventBus: EventBusService,
    private logger: LoggerService,
    private router: Router,
    @Inject(API_CONFIG) private apiConfig: ApiConfig
  ) {
    this.initializeAuthState();
  }

  login(credentials: LoginCredentials): Observable<AuthResponse> {
    this.logger.debug('AuthService: Attempting login', { email: credentials.email });
    
    return this.http.post<AuthResponse>(
      `${this.apiConfig.baseUrl}/auth/login`,
      credentials
    ).pipe(
      tap(response => {
        this.handleAuthentication(response, credentials.rememberMe || false);
        this.logger.info('AuthService: Login successful', { userId: response.user.id });
      }),
      catchError(error => {
        this.logger.error('AuthService: Login failed', error);
        this.eventBus.emit({ 
          type: EventBusService.SHOW_ERROR, 
          payload: 'Invalid email or password' 
        });
        return throwError(() => error);
      })
    );
  }

  logout(redirect: boolean = true): void {
    const refreshToken = this.storage.getItem<string>(this.STORAGE_KEYS.REFRESH_TOKEN);
    
    // Call logout API if we have a refresh token
    if (refreshToken) {
      this.http.post(`${this.apiConfig.baseUrl}/auth/logout`, { refreshToken })
        .pipe(
          catchError(error => {
            // Even if logout API fails, clear local state
            this.logger.warn('AuthService: Logout API call failed', error);
            return of(null);
          })
        )
        .subscribe();
    }

    this.clearAuthData();
    this.currentUserSubject.next(null);
    
    this.logger.info('AuthService: User logged out');
    
    if (redirect) {
      this.router.navigate(['/auth/login']);
    }
  }

  refreshToken(): Observable<string> {
    if (this.refreshTokenInProgress) {
      return this.refreshTokenSubject.pipe(
        switchMap(token => {
          if (token) {
            return of(token);
          } else {
            return throwError(() => new Error('Refresh token failed'));
          }
        })
      );
    }

    this.refreshTokenInProgress = true;
    this.refreshTokenSubject.next(null);

    const refreshToken = this.storage.getItem<string>(this.STORAGE_KEYS.REFRESH_TOKEN);
    
    if (!refreshToken) {
      this.logger.error('AuthService: No refresh token available');
      this.logout();
      return throwError(() => new Error('No refresh token'));
    }

    return this.http.post<{ accessToken: string }>(
      `${this.apiConfig.baseUrl}/auth/refresh`,
      { refreshToken }
    ).pipe(
      tap(response => {
        this.storage.setItem(this.STORAGE_KEYS.ACCESS_TOKEN, response.accessToken);
        this.refreshTokenInProgress = false;
        this.refreshTokenSubject.next(response.accessToken);
        this.logger.debug('AuthService: Token refreshed successfully');
      }),
      catchError(error => {
        this.refreshTokenInProgress = false;
        this.refreshTokenSubject.next(null);
        this.logger.error('AuthService: Token refresh failed', error);
        this.eventBus.emit({ type: EventBusService.AUTH_EXPIRED });
        this.logout();
        return throwError(() => error);
      }),
      map(response => response.accessToken)
    );
  }

  getAccessToken(): string | null {
    return this.storage.getItem<string>(this.STORAGE_KEYS.ACCESS_TOKEN);
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;

    const expiry = this.storage.getItem<number>(this.STORAGE_KEYS.TOKEN_EXPIRY);
    if (!expiry) return false;

    return Date.now() < expiry;
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user ? user.roles.includes(role) : false;
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUserSubject.value;
    return user ? user.permissions.includes(permission) : false;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  private initializeAuthState(): void {
    const user = this.storage.getItem<User>(this.STORAGE_KEYS.USER);
    if (user && this.isAuthenticated()) {
      this.currentUserSubject.next(user);
      this.logger.debug('AuthService: Auth state initialized from storage');
    } else {
      this.clearAuthData();
    }
  }

  private handleAuthentication(response: AuthResponse, rememberMe: boolean): void {
    const expiryTime = Date.now() + (response.expiresIn * 1000);
    
    this.storage.setItem(this.STORAGE_KEYS.ACCESS_TOKEN, response.accessToken);
    this.storage.setItem(this.STORAGE_KEYS.USER, response.user);
    this.storage.setItem(this.STORAGE_KEYS.TOKEN_EXPIRY, expiryTime);
    
    if (rememberMe) {
      this.storage.setItem(this.STORAGE_KEYS.REFRESH_TOKEN, response.refreshToken);
    } else {
      // Store refresh token in session storage for "remember me" = false
      this.storage.setItem(this.STORAGE_KEYS.REFRESH_TOKEN, response.refreshToken, false);
    }

    this.currentUserSubject.next(response.user);
  }

  private clearAuthData(): void {
    Object.values(this.STORAGE_KEYS).forEach(key => {
      this.storage.removeItem(key);
    });
  }
}