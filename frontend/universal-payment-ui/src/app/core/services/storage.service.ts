import { Injectable, Inject, PLATFORM_ID, InjectionToken } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { EncryptionService } from './encryption.service';

export interface StorageConfig {
  prefix: string;
}

export const STORAGE_CONFIG = new InjectionToken<StorageConfig>('STORAGE_CONFIG');

@Injectable({
  providedIn: 'root'
})
export class StorageService {
  private readonly isBrowser: boolean;

  constructor(
    @Inject(PLATFORM_ID) private platformId: any,
    @Inject(STORAGE_CONFIG) private config: StorageConfig,
    private encryption: EncryptionService
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  setItem(key: string, value: any, encrypt: boolean = true): void {
    if (!this.isBrowser) return;

    const storageKey = `${this.config.prefix}${key}`;
    let processedValue = typeof value === 'string' ? value : JSON.stringify(value);
    
    if (encrypt) {
      processedValue = this.encryption.encrypt(processedValue);
    }

    try {
      localStorage.setItem(storageKey, processedValue);
    } catch (error) {
      console.error('StorageService: Failed to set item', error);
      this.handleStorageError();
    }
  }

  getItem<T>(key: string, decrypt: boolean = true): T | null {
    if (!this.isBrowser) return null;

    const storageKey = `${this.config.prefix}${key}`;
    
    try {
      const item = localStorage.getItem(storageKey);
      if (!item) return null;

      let processedValue = item;
      if (decrypt) {
        processedValue = this.encryption.decrypt(item);
      }

      try {
        return JSON.parse(processedValue) as T;
      } catch {
        return processedValue as T;
      }
    } catch (error) {
      console.error('StorageService: Failed to get item', error);
      return null;
    }
  }

  removeItem(key: string): void {
    if (!this.isBrowser) return;

    const storageKey = `${this.config.prefix}${key}`;
    try {
      localStorage.removeItem(storageKey);
    } catch (error) {
      console.error('StorageService: Failed to remove item', error);
    }
  }

  clear(): void {
    if (!this.isBrowser) return;

    try {
      const keysToRemove: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key?.startsWith(this.config.prefix)) {
          keysToRemove.push(key);
        }
      }
      keysToRemove.forEach(key => localStorage.removeItem(key));
    } catch (error) {
      console.error('StorageService: Failed to clear storage', error);
    }
  }

  private handleStorageError(): void {
    try {
      this.clear();
    } catch (error) {
      console.error('StorageService: Critical storage error', error);
    }
  }
}