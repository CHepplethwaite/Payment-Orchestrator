import { Provider } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { API_CONFIG, STORAGE_CONFIG, environment } from './environment.config';
import { JwtInterceptor } from '../core/authentication/jwt.interceptor';
import { HttpErrorInterceptor } from '../core/interceptors/http-error.interceptor';

export const coreProviders: Provider[] = [
  { provide: API_CONFIG, useValue: environment.api },
  { provide: STORAGE_CONFIG, useValue: environment.storage },
  { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
  { provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, multi: true }
];