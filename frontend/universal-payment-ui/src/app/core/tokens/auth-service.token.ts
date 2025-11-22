import { InjectionToken } from '@angular/core';
import { AuthService } from '../authentication/auth.service';

export const AUTH_SERVICE = new InjectionToken<AuthService>('AuthService');