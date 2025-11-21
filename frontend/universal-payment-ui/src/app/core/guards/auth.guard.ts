import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { 
  CanActivate, 
  CanActivateChild, 
  CanLoad, 
  Router, 
  ActivatedRouteSnapshot, 
  RouterStateSnapshot, 
  Route, 
  UrlTree
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AuthService } from '../authentication/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {

  constructor(
    private authService: AuthService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: any
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | boolean | UrlTree {
    return this.checkAuthentication(route, state);
  }

  canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | boolean | UrlTree {
    return this.checkAuthentication(route, state);
  }

  canLoad(route: Route): Observable<boolean> | boolean {
    return this.checkAuthentication(undefined, undefined, route);
  }

  private checkAuthentication(
    route?: ActivatedRouteSnapshot,
    state?: RouterStateSnapshot,
    lazyRoute?: Route
  ): Observable<boolean | UrlTree> | boolean | UrlTree {

    // Not authenticated
    if (!this.authService.isAuthenticated) {
      return this.redirectToLogin(state);
    }

    const targetRoute = route || lazyRoute;

    // Not authorized
    if (targetRoute && !this.authService.canAccess(targetRoute)) {
      return this.redirectToForbidden(state);
    }

    // Token refresh (browser only)
    if (isPlatformBrowser(this.platformId)) {
      const token = this.authService.accessToken; // getter, no ()
      if (token && this.isTokenExpiringSoon(token)) {
        return this.authService.refreshToken().pipe(
          map(() => true),
          catchError(() => of(this.redirectToLogin(state)))
        );
      }
    }

    return true;
  }

  private redirectToLogin(state?: RouterStateSnapshot): UrlTree {
    return this.router.createUrlTree(['/auth/login'], {
      queryParams: state ? { returnUrl: state.url } : undefined
    });
  }

  private redirectToForbidden(state?: RouterStateSnapshot): UrlTree {
    return this.router.createUrlTree(['/error/403'], {
      queryParams: state ? { returnUrl: state.url } : undefined
    });
  }

  private isTokenExpiringSoon(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000; // milliseconds
      const now = Date.now();
      const bufferTime = 5 * 60 * 1000; // 5 min buffer
      return (exp - now) < bufferTime;
    } catch {
      return false;
    }
  }
}
