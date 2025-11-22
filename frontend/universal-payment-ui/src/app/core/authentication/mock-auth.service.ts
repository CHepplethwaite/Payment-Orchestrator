// src/app/core/authentication/mock-auth.service.ts
import { Injectable } from '@angular/core';
import { Observable, of, throwError, BehaviorSubject } from 'rxjs';
import { delay, map, tap } from 'rxjs/operators';

export interface User {
  id: number;
  email: string;
  password: string;
  verified: boolean;
  roles?: string[];
  twoFactorEnabled?: boolean;
  profileCompleted?: boolean;
  name?: string;
  avatar?: string;
  lastLogin?: Date;
}

// Fixed: Added proper type for method
export type TwoFactorMethod = 'totp' | 'sms' | 'email';

export interface LoginResponse {
  token: string;
  user?: User;
  requiresTwoFactor?: boolean;
  method?: TwoFactorMethod; // Use the specific type instead of string
}

export interface TwoFactorVerification {
  code: string;
  rememberDevice?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class MockAuthService {
  private users: User[] = [
    {
      id: 1,
      email: 'cliffordrh@gmail.com',
      password: '1234',
      verified: true,
      roles: ['user'],
      twoFactorEnabled: false,
      profileCompleted: true,
      name: 'Clifford Rhodes',
      avatar: 'assets/images/avatars/user1.jpg',
      lastLogin: new Date()
    },
    {
      id: 2,
      email: 'admin@example.com',
      password: 'admin123',
      verified: true,
      roles: ['user', 'admin'],
      twoFactorEnabled: true,
      profileCompleted: true,
      name: 'Admin User',
      lastLogin: new Date()
    },
    {
      id: 3,
      email: 'user-2fa@example.com',
      password: '123456',
      verified: true,
      roles: ['user'],
      twoFactorEnabled: true,
      profileCompleted: true,
      name: '2FA User',
      lastLogin: new Date()
    }
  ];

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  
  // Mock 2FA codes storage
  private twoFactorCodes = new Map<string, string>(); // email -> code
  private rememberedDevices = new Set<string>();

  constructor() {
    this.initializeFromStorage();
  }

  /** Returns the current logged-in user */
  get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  /** Observable for authentication state */
  get isAuthenticated$(): Observable<boolean> {
    return this.currentUser$.pipe(map(user => !!user));
  }

  /** Returns true if the current user is verified */
  isVerified(): boolean {
    return this.currentUserValue?.verified ?? false;
  }

  /** Check if current user has a specific role */
  hasRole(role: string): boolean {
    return this.currentUserValue?.roles?.includes(role) ?? false;
  }

  /** Check if user is authenticated */
  isAuthenticated(): boolean {
    return !!this.currentUserValue && !!this.getToken();
  }

  /** Get stored token */
  getToken(): string | null {
    return localStorage.getItem('mock_token');
  }

  /** Login with enhanced response for 2FA - FIXED TYPE ISSUE */
  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    const user = this.users.find(
      u => u.email === credentials.email && u.password === credentials.password
    );

    if (!user) {
      return throwError(() => new Error('Invalid email or password')).pipe(delay(800));
    }

    if (!user.verified) {
      return throwError(() => new Error('Please verify your email before logging in')).pipe(delay(800));
    }

    // If 2FA is enabled, return requiresTwoFactor response
    if (user.twoFactorEnabled) {
      // Store the user temporarily for 2FA verification
      this.currentUserSubject.next(user);
      
      // Generate and store a mock 2FA code
      const twoFactorCode = this.generateTwoFactorCode();
      this.twoFactorCodes.set(user.email, twoFactorCode);
      
      console.log(`[Mock Auth] 2FA Code for ${user.email}: ${twoFactorCode}`);

      // FIXED: Use the proper TwoFactorMethod type instead of string
      const response: LoginResponse = {
        requiresTwoFactor: true,
        method: 'totp', // This is now type-safe
        token: '' // No token until 2FA verified
      };

      return of(response).pipe(delay(800));
    }

    // Regular login success
    return this.completeLogin(user);
  }

  /** Complete login process */
  private completeLogin(user: User): Observable<LoginResponse> {
    const token = this.generateToken(user);
    user.lastLogin = new Date();
    
    // Update current user
    this.currentUserSubject.next(user);
    
    // Store in localStorage
    localStorage.setItem('mock_token', token);
    localStorage.setItem('mock_user', JSON.stringify(user));

    const response: LoginResponse = {
      token,
      user
    };

    return of(response).pipe(delay(800));
  }

  /** Verify 2FA code */
  verifyTwoFactorCode(code: string, rememberDevice: boolean = false): Observable<LoginResponse> {
    const user = this.currentUserValue;
    
    if (!user) {
      return throwError(() => new Error('No user found for 2FA verification')).pipe(delay(800));
    }

    const storedCode = this.twoFactorCodes.get(user.email);
    
    if (!storedCode || storedCode !== code) {
      return throwError(() => new Error('Invalid verification code')).pipe(delay(800));
    }

    // Clear the used code
    this.twoFactorCodes.delete(user.email);

    // Remember device if requested
    if (rememberDevice) {
      this.rememberedDevices.add(user.email);
      localStorage.setItem(`mock_remembered_${user.email}`, 'true');
    }

    return this.completeLogin(user);
  }

  /** Resend 2FA code */
  resendTwoFactorCode(): Observable<{ message: string }> {
    const user = this.currentUserValue;
    
    if (!user) {
      return throwError(() => new Error('No user found')).pipe(delay(800));
    }

    // Generate new code
    const newCode = this.generateTwoFactorCode();
    this.twoFactorCodes.set(user.email, newCode);
    
    console.log(`[Mock Auth] New 2FA Code for ${user.email}: ${newCode}`);

    return of({ 
      message: 'Verification code sent successfully' 
    }).pipe(delay(800));
  }

  /** Check if device is remembered */
  isDeviceRemembered(email: string): boolean {
    return this.rememberedDevices.has(email) || 
           localStorage.getItem(`mock_remembered_${email}`) === 'true';
  }

  /** Register */
  register(userData: { email: string; password: string; name?: string }): Observable<any> {
    const exists = this.users.find(u => u.email === userData.email);
    if (exists) {
      return throwError(() => new Error('Email already exists')).pipe(delay(800));
    }

    const newUser: User = {
      id: this.users.length + 1,
      email: userData.email,
      password: userData.password,
      verified: false,
      roles: ['user'],
      profileCompleted: false,
      name: userData.name,
      lastLogin: new Date()
    };

    this.users.push(newUser);
    
    // Auto-login after registration (optional)
    this.currentUserSubject.next(newUser);
    localStorage.setItem('mock_user', JSON.stringify(newUser));

    return of({ 
      message: 'Registration successful. Please verify your email.',
      user: newUser 
    }).pipe(delay(800));
  }

  /** Logout */
  logout(): void {
    this.currentUserSubject.next(null);
    localStorage.removeItem('mock_token');
    localStorage.removeItem('mock_user');
  }

  /** Forgot password */
  forgotPassword(email: string): Observable<any> {
    const user = this.users.find(u => u.email === email);
    if (!user) {
      return throwError(() => new Error('Email not found')).pipe(delay(800));
    }
    
    // Generate reset token (mock)
    const resetToken = this.generateToken(user);
    localStorage.setItem(`mock_reset_${email}`, resetToken);
    
    console.log(`[Mock Auth] Password reset token for ${email}: ${resetToken}`);
    
    return of({ 
      message: 'Password reset email sent',
      resetToken
    }).pipe(delay(800));
  }

  /** Reset password */
  resetPassword(token: string, newPassword: string): Observable<any> {
    const email = this.findEmailByResetToken(token);
    if (!email) {
      return throwError(() => new Error('Invalid or expired reset token')).pipe(delay(800));
    }

    const user = this.users.find(u => u.email === email);
    if (!user) {
      return throwError(() => new Error('User not found')).pipe(delay(800));
    }

    user.password = newPassword;
    localStorage.removeItem(`mock_reset_${email}`);

    return of({ message: 'Password reset successful' }).pipe(delay(800));
  }

  /** Verify email */
  verifyEmail(token: string): Observable<any> {
    const user = this.users.find(u => !u.verified) || this.users[0];
    user.verified = true;
    
    if (this.currentUserValue && this.currentUserValue.email === user.email) {
      this.currentUserSubject.next({ ...this.currentUserValue, verified: true });
    }

    return of({ message: 'Email verified successfully' }).pipe(delay(800));
  }

  /** Resend verification */
  resendVerification(email: string): Observable<any> {
    const user = this.users.find(u => u.email === email);
    if (!user) {
      return throwError(() => new Error('Email not found')).pipe(delay(800));
    }
    
    if (user.verified) {
      return throwError(() => new Error('Email is already verified')).pipe(delay(800));
    }

    const verifyToken = this.generateToken(user);
    console.log(`[Mock Auth] Verification token for ${email}: ${verifyToken}`);

    return of({ message: 'Verification email resent' }).pipe(delay(800));
  }

  /** Enable Two-Factor Authentication */
  enableTwoFactor(): Observable<any> {
    const user = this.currentUserValue;
    if (!user) {
      return throwError(() => new Error('User not found')).pipe(delay(800));
    }

    user.twoFactorEnabled = true;
    
    const index = this.users.findIndex(u => u.id === user.id);
    if (index !== -1) {
      this.users[index] = { ...user, twoFactorEnabled: true };
    }

    const backupCodes = this.generateBackupCodes();
    console.log(`[Mock Auth] 2FA Backup codes for ${user.email}:`, backupCodes);

    return of({ 
      message: '2FA enabled successfully',
      backupCodes,
      secret: 'MOCK_SECRET_KEY_FOR_2FA'
    }).pipe(delay(800));
  }

  /** Disable Two-Factor Authentication */
  disableTwoFactor(): Observable<any> {
    const user = this.currentUserValue;
    if (!user) {
      return throwError(() => new Error('User not found')).pipe(delay(800));
    }

    user.twoFactorEnabled = false;
    
    const index = this.users.findIndex(u => u.id === user.id);
    if (index !== -1) {
      this.users[index] = { ...user, twoFactorEnabled: false };
    }

    return of({ message: '2FA disabled successfully' }).pipe(delay(800));
  }

  /** Social login mock */
  socialLogin(provider: string): Observable<LoginResponse> {
    const socialEmail = `social_${provider}@example.com`;
    let user = this.users.find(u => u.email === socialEmail);
    
    if (!user) {
      user = {
        id: this.users.length + 1,
        email: socialEmail,
        password: 'social_login',
        verified: true,
        roles: ['user'],
        twoFactorEnabled: false,
        profileCompleted: true,
        name: `${provider} User`,
        lastLogin: new Date()
      };
      this.users.push(user);
    }

    return this.completeLogin(user);
  }

  /** Update user profile */
  updateProfile(profileData: Partial<User>): Observable<any> {
    const user = this.currentUserValue;
    if (!user) {
      return throwError(() => new Error('User not found')).pipe(delay(800));
    }

    const updatedUser = { ...user, ...profileData, profileCompleted: true };
    
    const index = this.users.findIndex(u => u.id === user.id);
    if (index !== -1) {
      this.users[index] = updatedUser;
    }

    this.currentUserSubject.next(updatedUser);
    localStorage.setItem('mock_user', JSON.stringify(updatedUser));

    return of({ 
      message: 'Profile updated successfully',
      user: updatedUser 
    }).pipe(delay(800));
  }

  /** Check if email exists */
  checkEmailExists(email: string): Observable<{ exists: boolean }> {
    const exists = this.users.some(u => u.email === email);
    return of({ exists }).pipe(delay(300));
  }

  /** Initialize from storage */
  private initializeFromStorage(): void {
    const storedUser = localStorage.getItem('mock_user');
    const token = localStorage.getItem('mock_token');
    
    if (storedUser && token) {
      try {
        const user = JSON.parse(storedUser);
        this.currentUserSubject.next(user);
      } catch (e) {
        console.error('Failed to parse stored user:', e);
        this.clearStorage();
      }
    }
  }

  /** Clear storage */
  private clearStorage(): void {
    localStorage.removeItem('mock_token');
    localStorage.removeItem('mock_user');
  }

  /** Generate mock token */
  private generateToken(user: User): string {
    return `mock_jwt_${user.id}_${Date.now()}`;
  }

  /** Generate 2FA code */
  private generateTwoFactorCode(): string {
    return Math.floor(100000 + Math.random() * 900000).toString();
  }

  /** Generate backup codes */
  private generateBackupCodes(): string[] {
    return Array.from({ length: 8 }, () => 
      Math.random().toString(36).substring(2, 8).toUpperCase()
    );
  }

  /** Find email by reset token (mock) */
  private findEmailByResetToken(token: string): string | null {
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key?.startsWith('mock_reset_')) {
        const storedToken = localStorage.getItem(key);
        if (storedToken === token) {
          return key.replace('mock_reset_', '');
        }
      }
    }
    return null;
  }
}