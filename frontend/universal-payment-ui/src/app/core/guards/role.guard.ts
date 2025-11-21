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
import { AuthService } from '../authentication/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate, CanActivateChild {
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkRoleAccess(route, state);
  }

  canActivateChild(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkRoleAccess(route, state);
  }

  private checkRoleAccess(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean | UrlTree {
    
    // If no roles are specified, allow access
    const requiredRoles = this.getRequiredRoles(route);
    if (!requiredRoles || requiredRoles.length === 0) {
      return true;
    }

    // Check if user has any of the required roles
    const hasRequiredRole = this.authService.hasRole(requiredRoles);
    
    if (!hasRequiredRole) {
      console.warn(
        `RoleGuard: Access denied. Required roles: ${requiredRoles.join(', ')}. ` +
        `User roles: ${this.authService.currentUserValue?.roles?.join(', ') || 'none'}`
      );
      
      return this.router.createUrlTree(['/error/403'], {
        queryParams: { returnUrl: state.url }
      });
    }

    return true;
  }

  private getRequiredRoles(route: ActivatedRouteSnapshot): string[] {
    // Get roles from route data
    const routeRoles = route.data['roles'] as string[];
    
    // Get roles from parent routes (if any)
    let parentRoles: string[] = [];
    let parent = route.parent;
    while (parent) {
      const parentRouteRoles = parent.data['roles'] as string[];
      if (parentRouteRoles) {
        parentRoles = [...parentRoles, ...parentRouteRoles];
      }
      parent = parent.parent;
    }

    // Combine and deduplicate roles
    const allRoles = [...(routeRoles || []), ...parentRoles];
    return [...new Set(allRoles)];
  }
}