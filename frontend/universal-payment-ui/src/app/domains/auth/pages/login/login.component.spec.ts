import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthenticationService } from '../../../core/services/authentication.service';
import { NotificationService } from '../../../../shared/services/notification.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthenticationService>;
  let notificationService: jasmine.SpyObj<NotificationService>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthenticationService', ['login', 'currentUserValue']);
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', ['showSuccess', 'showError']);

    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ReactiveFormsModule, RouterTestingModule],
      providers: [
        { provide: AuthenticationService, useValue: authServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthenticationService) as jasmine.SpyObj<AuthenticationService>;
    notificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty values', () => {
    expect(component.loginForm.value).toEqual({
      email: '',
      password: '',
      rememberMe: false
    });
  });

  it('should validate email as required', () => {
    const emailControl = component.loginForm.controls.email;
    expect(emailControl.valid).toBeFalse();

    emailControl.setValue('test@example.com');
    expect(emailControl.valid).toBeTrue();
  });

  it('should validate email format', () => {
    const emailControl = component.loginForm.controls.email;
    emailControl.setValue('invalid-email');
    expect(emailControl.valid).toBeFalse();

    emailControl.setValue('valid@example.com');
    expect(emailControl.valid).toBeTrue();
  });

  it('should validate password as required', () => {
    const passwordControl = component.loginForm.controls.password;
    expect(passwordControl.valid).toBeFalse();

    passwordControl.setValue('password123');
    expect(passwordControl.valid).toBeTrue();
  });

  it('should mark all controls as touched when form is invalid and submitted', () => {
    component.onSubmit();
    expect(component.loginForm.controls.email.touched).toBeTrue();
    expect(component.loginForm.controls.password.touched).toBeTrue();
  });

  it('should call authService login when form is valid', fakeAsync(() => {
    const loginResponse = { user: { id: 1, email: 'test@example.com' }, token: 'jwt-token' };
    authService.login.and.returnValue(of(loginResponse));

    component.loginForm.controls.email.setValue('test@example.com');
    component.loginForm.controls.password.setValue('password123');
    
    component.onSubmit();
    tick();

    expect(authService.login).toHaveBeenCalledWith('test@example.com', 'password123', false);
    expect(notificationService.showSuccess).toHaveBeenCalledWith('Login successful!');
  }));

  it('should handle login error', fakeAsync(() => {
    const error = { message: 'Invalid credentials' };
    authService.login.and.returnValue(throwError(error));

    component.loginForm.controls.email.setValue('test@example.com');
    component.loginForm.controls.password.setValue('wrongpassword');
    
    component.onSubmit();
    tick();

    expect(authService.login).toHaveBeenCalled();
    expect(notificationService.showError).toHaveBeenCalledWith('Invalid credentials');
    expect(component.loading).toBeFalse();
  }));

  it('should toggle password visibility', () => {
    expect(component.showPassword).toBeFalse();
    component.togglePasswordVisibility();
    expect(component.showPassword).toBeTrue();
    component.togglePasswordVisibility();
    expect(component.showPassword).toBeFalse();
  });
});