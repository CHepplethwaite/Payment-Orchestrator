import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService, Notification } from './notification.service';
import { NotificationComponent } from './notification.component';

@Component({
  selector: 'app-notification-container',
  standalone: true,
  imports: [CommonModule, NotificationComponent],
  template: `
    <div class="notification-container">
      <app-notification
        *ngFor="let notification of notifications()"
        [notification]="notification"
        (dismiss)="removeNotification(notification.id)"
        (action)="handleAction(notification)">
      </app-notification>
    </div>
  `,
  styleUrls: ['./notification-container.component.css']
})
export class NotificationContainerComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  
  notifications = this.notificationService.notificationsList;

  ngOnInit(): void {
    // Component initialization if needed
  }

  removeNotification(id: string): void {
    this.notificationService.removeNotification(id);
  }

  handleAction(notification: Notification): void {
    // Action is already handled in the individual notification component
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }
}