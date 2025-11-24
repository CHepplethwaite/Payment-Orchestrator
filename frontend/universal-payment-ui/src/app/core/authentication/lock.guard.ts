import { Injectable } from '@angular/core';
import { 
  CanActivate, 
  ActivatedRouteSnapshot, 
  RouterStateSnapshot, 
  Router, 
  UrlTree 
} from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';

// Adjust import path as needed
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class LockGuard implements CanActivate {
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    
    return this.authService.currentUser.pipe(
      take(1),
      map(user => {
        // If no user is logged in, allow access to the route
        if (!user) {
          return true;
        }

        // Check if account is locked (you might need to add this property to your User interface)
        // For now, we'll assume there's an isLocked property or similar check
        const isAccountLocked = this.checkIfAccountLocked(user);
        
        if (isAccountLocked) {
          // Redirect to account locked page
          return this.router.createUrlTree(['/auth/account-locked'], {
            queryParams: { returnUrl: state.url }
          });
        }

        // Account is not locked, allow access
        return true;
      })
    );
  }

  /**
   * Check if the user's account is locked
   * You can implement your own logic here based on your backend response
   */
  private checkIfAccountLocked(user: any): boolean {
    // Example checks (adjust based on your User model):
    
    // 1. Check if account is explicitly locked
    if (user.isLocked) {
      return true;
    }

    // 2. Check if login attempts exceeded
    if (user.failedLoginAttempts >= 5) {
      return true;
    }

    // 3. Check if account is temporarily suspended
    if (user.suspendedUntil && new Date(user.suspendedUntil) > new Date()) {
      return true;
    }

    // 4. Add any other business logic for account locking

    return false;
  }
}