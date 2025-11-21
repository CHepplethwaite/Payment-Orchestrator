import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { CanActivate, CanActivateChild, CanLoad, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export interface GuestGuardConfig {
  // Allow access even if authenticated (useful for public pages that authenticated users can still access)
  allowAuthenticated?: boolean;
  
  // Specific roles that are allowed (empty = all roles treated as guests)
  allowedRoles?: string[];
  
  // Redirect route for authenticated users
  redirectTo?: string;
  
  // Whether to preserve the attempted URL for later use
  preserveAttemptedUrl?: boolean;
  
  // Query parameters to pass to redirect
  redirectQueryParams?: { [key: string]: any };
}

@Injectable({
  providedIn: 'root'
})
export class GuestGuard implements CanActivate, CanActivateChild, CanLoad {
  constructor(
    private authService: AuthService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkGuestAccess(route, state.url);
  }

  canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkGuestAccess(route, state.url);
  }

  canLoad(): Observable<boolean> | Promise<boolean> | boolean {
    return this.checkGuestAccess();
  }

  private checkGuestAccess(route?: ActivatedRouteSnapshot, redirectUrl?: string): Observable<boolean> | boolean {
    // Server-side rendering support
    if (!isPlatformBrowser(this.platformId)) {
      return true;
    }

    const config: GuestGuardConfig = this.getGuardConfig(route);
    
    // If authenticated users are allowed, grant access immediately
    if (config.allowAuthenticated && this.authService.isAuthenticated()) {
      return true;
    }

    // Check authentication status
    if (this.authService.isAuthenticated()) {
      const user = this.authService.currentUserValue;
      
      // Check role-based access if specified
      if (config.allowedRoles && config.allowedRoles.length > 0) {
        if (user && config.allowedRoles.some(role => this.authService.hasRole(role))) {
          return true;
        }
      }

      // Redirect authenticated users
      return this.handleAuthenticatedUser(user, config, redirectUrl);
    }

    // Allow access for guests
    return true;
  }

  private handleAuthenticatedUser(user: any, config: GuestGuardConfig, attemptedUrl?: string): boolean {
    // Preserve attempted URL if configured
    if (config.preserveAttemptedUrl && attemptedUrl) {
      this.storeAttemptedUrl(attemptedUrl);
    }

    // Determine redirect destination
    const redirectTo = config.redirectTo || this.getDefaultRedirectRoute(user);
    
    // Create navigation extras with query params
    const navigationExtras: any = {};
    if (config.redirectQueryParams) {
      navigationExtras.queryParams = config.redirectQueryParams;
    }

    // Perform redirect
    this.router.navigate([redirectTo], navigationExtras);
    return false;
  }

  private getGuardConfig(route?: ActivatedRouteSnapshot): GuestGuardConfig {
    const defaultConfig: GuestGuardConfig = {
      allowAuthenticated: false,
      allowedRoles: [],
      redirectTo: undefined,
      preserveAttemptedUrl: false,
      redirectQueryParams: undefined
    };

    if (!route) {
      return defaultConfig;
    }

    // Get configuration from route data
    const routeConfig = route.data['guestGuard'] as GuestGuardConfig;
    
    if (!routeConfig) {
      return defaultConfig;
    }

    return {
      ...defaultConfig,
      ...routeConfig
    };
  }

  private getDefaultRedirectRoute(user: any): string {
    // Customize this based on your application's requirements
    
    if (user?.roles?.includes('admin')) {
      return '/admin/dashboard';
    }
    
    if (user?.roles?.includes('moderator')) {
      return '/moderator/dashboard';
    }

    // Check if user needs to complete profile
    if (!user?.profileCompleted) {
      return '/profile/complete';
    }

    // Check if user needs email verification
    if (!user?.isVerified) {
      return '/auth/verification-required';
    }

    // Default dashboard
    return '/dashboard';
  }

  private storeAttemptedUrl(url: string): void {
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.setItem('guest_guard_attempted_url', url);
    }
  }

  public getStoredAttemptedUrl(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return sessionStorage.getItem('guest_guard_attempted_url');
    }
    return null;
  }

  public clearStoredAttemptedUrl(): void {
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.removeItem('guest_guard_attempted_url');
    }
  }
}