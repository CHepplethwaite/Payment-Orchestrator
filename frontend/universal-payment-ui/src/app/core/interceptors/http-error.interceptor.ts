import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { EventBusService } from '../services/event-bus.service';
import { LoggerService } from '../services/logger.service';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {

  constructor(
    private eventBus: EventBusService,
    private logger: LoggerService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        let errorMessage = 'An unexpected error occurred';
        let userFriendlyMessage = 'Something went wrong. Please try again.';

        if (error.error instanceof ErrorEvent) {
          // Client-side error
          errorMessage = `Client Error: ${error.error.message}`;
          userFriendlyMessage = 'Network error occurred. Please check your connection.';
        } else {
          // Server-side error
          errorMessage = `Server Error: ${error.status} - ${error.message}`;
          userFriendlyMessage = this.getUserFriendlyMessage(error);
        }

        // Log the error
        this.logger.error(`HTTP Error [${request.method} ${request.url}]`, {
          error: errorMessage,
          status: error.status,
          body: error.error
        });

        // Emit event for global error handling
        this.eventBus.emit({
          type: EventBusService.HTTP_ERROR,
          payload: {
            message: userFriendlyMessage,
            originalError: error,
            request: {
              method: request.method,
              url: request.url
            }
          }
        });

        return throwError(() => error);
      })
    );
  }

  private getUserFriendlyMessage(error: HttpErrorResponse): string {
    switch (error.status) {
      case 0:
        return 'Unable to connect to server. Please check your network connection.';
      case 400:
        return 'Invalid request. Please check your input and try again.';
      case 401:
        return 'Your session has expired. Please log in again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 409:
        return 'This action conflicts with existing data.';
      case 429:
        return 'Too many requests. Please wait a moment and try again.';
      case 500:
        return 'Server error. Our team has been notified.';
      case 503:
        return 'Service temporarily unavailable. Please try again later.';
      default:
        return 'An unexpected error occurred. Please try again.';
    }
  }
}