import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideClientHydration } from '@angular/platform-browser';
import { JWT_OPTIONS, JwtHelperService } from '@auth0/angular-jwt';

import { routes } from './app.routes';
import { AUTH_SERVICE } from './core/tokens/auth-service.token';
import { AuthService } from './core/authentication/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(),
    provideClientHydration(),
    
    // JWT Service
    { provide: JWT_OPTIONS, useValue: {} },
    JwtHelperService,
    
    // AuthService with injection token
    {
      provide: AUTH_SERVICE,
      useExisting: AuthService // Use existing instance since AuthService is providedIn: 'root'
    },
    
    // Guards (they're providedIn: 'root' but you can also list them here if needed)
  ]
};