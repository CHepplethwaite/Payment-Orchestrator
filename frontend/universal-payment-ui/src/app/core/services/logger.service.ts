import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export enum LogLevel {
  ERROR = 0,
  WARN = 1,
  INFO = 2,
  DEBUG = 3
}

@Injectable({
  providedIn: 'root'
})
export class LoggerService {
  private logLevel: LogLevel = LogLevel.INFO;
  private readonly isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: any) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    
    // In development, show all logs
    if (this.isBrowser && !window.location.href.includes('prod')) {
      this.logLevel = LogLevel.DEBUG;
    }
  }

  debug(message: string, ...args: any[]): void {
    this.log(LogLevel.DEBUG, message, args);
  }

  info(message: string, ...args: any[]): void {
    this.log(LogLevel.INFO, message, args);
  }

  warn(message: string, ...args: any[]): void {
    this.log(LogLevel.WARN, message, args);
  }

  error(message: string, ...args: any[]): void {
    this.log(LogLevel.ERROR, message, args);
  }

  private log(level: LogLevel, message: string, args: any[]): void {
    if (level > this.logLevel) return;

    const timestamp = new Date().toISOString();
    const logMessage = `[${timestamp}] ${message}`;

    switch (level) {
      case LogLevel.DEBUG:
        console.debug(logMessage, ...args);
        break;
      case LogLevel.INFO:
        console.info(logMessage, ...args);
        break;
      case LogLevel.WARN:
        console.warn(logMessage, ...args);
        break;
      case LogLevel.ERROR:
        console.error(logMessage, ...args);
        break;
    }

    // In production, send errors to logging service
    if (level === LogLevel.ERROR && this.isBrowser) {
      this.sendToLoggingService(logMessage, args);
    }
  }

  private sendToLoggingService(message: string, args: any[]): void {
    // Implement your logging service integration here
    // e.g., Sentry, LogRocket, etc.
    try {
      // Example: Send to external logging service
      if ((window as any).Sentry) {
        (window as any).Sentry.captureMessage(message, {
          extra: { args }
        });
      }
    } catch (error) {
      // Avoid infinite loop if logging fails
      console.error('LoggerService: Failed to send to logging service', error);
    }
  }
}