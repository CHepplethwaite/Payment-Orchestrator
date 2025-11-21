import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/authentication/auth.service';
import { NotificationService } from '../../../../shared/services/notification.service';

@Component({
  selector: 'app-logout',
  template: `
    <div class="logout-container">
      <p>Logging out...</p>
    </div>
  `
})
export class LogoutComponent implements OnInit {

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    this.performLogout();
  }

  private performLogout() {
    this.authService.logout();
    this.notificationService.showSuccess('You have been logged out successfully.');
    this.router.navigate(['/auth/login']);
  }
}