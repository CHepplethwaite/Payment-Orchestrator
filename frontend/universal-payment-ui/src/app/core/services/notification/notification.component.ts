import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Notification } from './notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div 
      class="notification"
      [class]="'notification--' + notification().type"
      [class.notification--dismissed]="isDismissed">
      
      <div class="notification__icon">
        <ng-container [ngSwitch]="notification().type">
          <span *ngSwitchCase="'success'">✓</span>
          <span *ngSwitchCase="'error'">⚠</span>
          <span *ngSwitchCase="'warning'">⚠</span>
          <span *ngSwitchDefault>ℹ</span>
        </ng-container>
      </div>

      <div class="notification__content">
        <h4 class="notification__title">{{ notification().title }}</h4>
        <p class="notification__message">{{ notification().message }}</p>
        
        <div *ngIf="notification().action" class="notification__actions">
          <button 
            type="button" 
            class="notification__action"
            (click)="onAction()">
            {{ notification().action!.label }}
          </button>
        </div>
      </div>

      <button 
        type="button" 
        class="notification__close"
        (click)="onClose()"
        aria-label="Close notification">
        ×
      </button>

      <div 
        *ngIf="notification().duration && notification().autoDismiss"
        class="notification__progress"
        [style.animation-duration]="notification().duration + 'ms'">
      </div>
    </div>
  `,
  styleUrls: ['./notification.component.css']
})
export class NotificationComponent {
  notification = input.required<Notification>();
  dismiss = output<void>();
  action = output<void>();

  isDismissed = false;

  onClose(): void {
    this.isDismissed = true;
    setTimeout(() => this.dismiss.emit(), 300);
  }

  onAction(): void {
    this.notification().action?.callback();
    this.onClose();
  }
}