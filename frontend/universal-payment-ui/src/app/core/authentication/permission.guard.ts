import { Injectable } from '@angular/core';
import { 
  CanActivate, 
  CanActivateChild, 
  Router, 
  ActivatedRouteSnapshot, 
  RouterStateSnapshot, 
  UrlTree 
} from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface PermissionConfig {
  requireAll?: boolean; // AND operation instead of OR
  redirectTo?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate, CanActivateChild {
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkPermissionAccess(route, state);
  }

  canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkPermissionAccess(route, state);
  }

  private checkPermissionAccess(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean | UrlTree {
    
    const config = this.getPermissionConfig(route);
    const requiredPermissions = this.getRequiredPermissions(route);
    
    // If no permissions are specified, allow access
    if (!requiredPermissions || requiredPermissions.length === 0) {
      return true;
    }

    // Check permissions based on configuration
    const hasPermission = config.requireAll 
      ? this.authService.hasPermission(requiredPermissions) // AND operation
      : requiredPermissions.some(permission => 
          this.authService.hasPermission(permission)
        ); // OR operation

    if (!hasPermission) {
      console.warn(
        `PermissionGuard: Access denied. Required permissions: ${requiredPermissions.join(', ')}. ` +
        `User permissions: ${this.authService.currentUserValue?.permissions?.join(', ') || 'none'}`
      );

      const redirectUrl = config.redirectTo || '/error/403';
      return this.router.createUrlTree([redirectUrl], {
        queryParams: { returnUrl: state.url }
      });
    }

    return true;
  }

  private getRequiredPermissions(route: ActivatedRouteSnapshot): string[] {
    // Get permissions from route data
    const routePermissions = route.data['permissions'] as string[];
    
    // Get permissions from parent routes (if any)
    let parentPermissions: string[] = [];
    let parent = route.parent;
    while (parent) {
      const parentRoutePermissions = parent.data['permissions'] as string[];
      if (parentRoutePermissions) {
        parentPermissions = [...parentPermissions, ...parentRoutePermissions];
      }
      parent = parent.parent;
    }

    // Combine and deduplicate permissions
    const allPermissions = [...(routePermissions || []), ...parentPermissions];
    return [...new Set(allPermissions)];
  }

  private getPermissionConfig(route: ActivatedRouteSnapshot): PermissionConfig {
    const config: PermissionConfig = {
      requireAll: false,
      redirectTo: '/error/403'
    };

    const routeConfig = route.data['permissionConfig'] as PermissionConfig;
    if (routeConfig) {
      return { ...config, ...routeConfig };
    }

    return config;
  }
}