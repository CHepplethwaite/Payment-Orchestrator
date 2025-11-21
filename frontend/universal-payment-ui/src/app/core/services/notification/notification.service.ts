import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  action?: {
    label: string;
    callback: () => void;
  };
  timestamp: Date;
}

export interface NotificationOptions {
  duration?: number;
  action?: {
    label: string;
    callback: () => void;
  };
  autoDismiss?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private router = inject(Router);
  
  // Using Angular 19 signals for state management
  private notifications = signal<Notification[]>([]);
  public readonly notificationsList = this.notifications.asReadonly();
  
  // Auto-remove notifications after their duration
  private autoRemoveTimers = new Map<string, NodeJS.Timeout>();

  /**
   * Show a success notification
   */
  showSuccess(message: string, title?: string, options?: NotificationOptions): string {
    return this.addNotification({
      type: 'success',
      title: title || 'Success',
      message,
      ...this.getDefaultOptions(options)
    });
  }

  /**
   * Show an error notification
   */
  showError(message: string, title?: string, options?: NotificationOptions): string {
    return this.addNotification({
      type: 'error',
      title: title || 'Error',
      message,
      ...this.getDefaultOptions(options)
    });
  }

  /**
   * Show a warning notification
   */
  showWarning(message: string, title?: string, options?: NotificationOptions): string {
    return this.addNotification({
      type: 'warning',
      title: title || 'Warning',
      message,
      ...this.getDefaultOptions(options)
    });
  }

  /**
   * Show an info notification
   */
  showInfo(message: string, title?: string, options?: NotificationOptions): string {
    return this.addNotification({
      type: 'info',
      title: title || 'Information',
      message,
      ...this.getDefaultOptions(options)
    });
  }

  /**
   * Authentication-specific notification methods
   */
  showAuthSuccess(message: string, action?: string): string {
    const options: NotificationOptions = {
      duration: 5000,
      action: action ? {
        label: 'View Dashboard',
        callback: () => this.router.navigate(['/dashboard'])
      } : undefined
    };

    return this.showSuccess(message, 'Authentication Successful', options);
  }

  showAuthError(error: any, context: string = 'authentication'): string {
    let message = 'An unexpected error occurred';
    let title = 'Authentication Failed';

    // Handle different types of authentication errors
    if (typeof error === 'string') {
      message = error;
    } else if (error?.error?.message) {
      message = error.error.message;
    } else if (error?.message) {
      message = error.message;
    }

    // Context-specific titles
    const contextTitles: { [key: string]: string } = {
      'login': 'Login Failed',
      'register': 'Registration Failed',
      'logout': 'Logout Failed',
      'verification': 'Verification Failed',
      'password-reset': 'Password Reset Failed',
      'two-factor': 'Two-Factor Authentication Failed'
    };

    title = contextTitles[context] || title;

    return this.showError(message, title, {
      duration: 7000,
      autoDismiss: false
    });
  }

  showSessionExpired(): string {
    return this.showWarning(
      'Your session has expired. Please log in again to continue.',
      'Session Expired',
      {
        duration: 10000,
        action: {
          label: 'Login',
          callback: () => this.router.navigate(['/auth/login'])
        },
        autoDismiss: false
      }
    );
  }

  showPermissionDenied(): string {
    return this.showError(
      'You do not have permission to access this resource.',
      'Access Denied',
      {
        duration: 6000,
        autoDismiss: true
      }
    );
  }

  /**
   * Remove a notification by ID
   */
  removeNotification(id: string): void {
    this.notifications.update(notifications => 
      notifications.filter(notification => notification.id !== id)
    );
    
    // Clear auto-remove timer if exists
    this.clearAutoRemoveTimer(id);
  }

  /**
   * Clear all notifications
   */
  clearAll(): void {
    // Clear all timers
    this.autoRemoveTimers.forEach((timer, id) => {
      clearTimeout(timer);
    });
    this.autoRemoveTimers.clear();
    
    // Clear notifications
    this.notifications.set([]);
  }

  /**
   * Clear notifications by type
   */
  clearByType(type: Notification['type']): void {
    this.notifications.update(notifications => {
      const toRemove = notifications.filter(n => n.type === type);
      
      // Clear timers for removed notifications
      toRemove.forEach(notification => {
        this.clearAutoRemoveTimer(notification.id);
      });
      
      return notifications.filter(n => n.type !== type);
    });
  }

  /**
   * Get notifications count by type
   */
  getNotificationCount(type?: Notification['type']): number {
    const current = this.notifications();
    if (type) {
      return current.filter(n => n.type === type).length;
    }
    return current.length;
  }

  /**
   * Private methods
   */
  private addNotification(notification: Omit<Notification, 'id' | 'timestamp'>): string {
    const id = this.generateId();
    const fullNotification: Notification = {
      ...notification,
      id,
      timestamp: new Date()
    };

    this.notifications.update(notifications => 
      [fullNotification, ...notifications].slice(0, 5) // Keep max 5 notifications
    );

    // Set up auto-dismiss if enabled
    if (notification.autoDismiss !== false && notification.duration) {
      this.setAutoRemoveTimer(id, notification.duration);
    }

    return id;
  }

  private setAutoRemoveTimer(id: string, duration: number): void {
    const timer = setTimeout(() => {
      this.removeNotification(id);
      this.autoRemoveTimers.delete(id);
    }, duration);

    this.autoRemoveTimers.set(id, timer);
  }

  private clearAutoRemoveTimer(id: string): void {
    const timer = this.autoRemoveTimers.get(id);
    if (timer) {
      clearTimeout(timer);
      this.autoRemoveTimers.delete(id);
    }
  }

  private generateId(): string {
    return `notification_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private getDefaultOptions(options?: NotificationOptions): Partial<Notification> {
    const defaultDuration: { [key: string]: number } = {
      success: 5000,
      error: 7000,
      warning: 6000,
      info: 4000
    };

    return {
      duration: options?.duration || defaultDuration[options?.type || 'info'],
      action: options?.action,
      autoDismiss: options?.autoDismiss ?? true
    };
  }
}