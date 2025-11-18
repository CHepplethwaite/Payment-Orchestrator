import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { LoggerService } from '../services/logger.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {

  constructor(
    private auth: AuthService,
    private router: Router,
    private logger: LoggerService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    
    const expectedRoles = route.data['roles'] as string[];
    const expectedPermissions = route.data['permissions'] as string[];

    // First check if user is authenticated
    if (!this.auth.isAuthenticated()) {
      this.logger.warn('RoleGuard: Access denied - user not authenticated', { 
        route: state.url,
        expectedRoles,
        expectedPermissions
      });
      return this.router.createUrlTree(['/auth/login'], {
        queryParams: { returnUrl: state.url }
      });
    }

    const currentUser = this.auth.getCurrentUser();

    // Check roles if specified
    if (expectedRoles && expectedRoles.length > 0) {
      const hasRole = expectedRoles.some(role => this.auth.hasRole(role));
      if (!hasRole) {
        this.logger.warn('RoleGuard: Access denied - insufficient roles', { 
          route: state.url,
          expectedRoles,
          userRoles: currentUser?.roles
        });
        return this.router.createUrlTree(['/unauthorized']);
      }
    }

    // Check permissions if specified
    if (expectedPermissions && expectedPermissions.length > 0) {
      const hasPermission = expectedPermissions.some(permission => 
        this.auth.hasPermission(permission)
      );
      if (!hasPermission) {
        this.logger.warn('RoleGuard: Access denied - insufficient permissions', { 
          route: state.url,
          expectedPermissions,
          userPermissions: currentUser?.permissions
        });
        return this.router.createUrlTree(['/unauthorized']);
      }
    }

    this.logger.debug('RoleGuard: Access granted', { 
      route: state.url,
      expectedRoles,
      expectedPermissions
    });
    
    return true;
  }
}