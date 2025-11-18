import { Injectable, Inject, InjectionToken } from '@angular/core';

export interface StorageConfig {
  encryptionKey: string;
  prefix?: string;
}

export const STORAGE_CONFIG = new InjectionToken<StorageConfig>('STORAGE_CONFIG');

@Injectable({
  providedIn: 'root'
})
export class EncryptionService {
  constructor(@Inject(STORAGE_CONFIG) private config: StorageConfig) {}

  encrypt(value: string): string {
    try {
      // Simple base64 encoding for demo - replace with proper encryption in production
      const encodedValue = btoa(unescape(encodeURIComponent(value)));
      return `enc_${this.config.encryptionKey}_${encodedValue}`;
    } catch (error) {
      console.error('EncryptionService: Encryption failed', error);
      return value;
    }
  }

  decrypt(value: string): string {
    try {
      if (!value.startsWith('enc_')) {
        return value; // Not encrypted, return as is
      }
      
      const parts = value.split('_');
      if (parts.length < 3) {
        return value;
      }
      
      const encodedValue = parts[2];
      return decodeURIComponent(escape(atob(encodedValue)));
    } catch (error) {
      console.error('EncryptionService: Decryption failed', error);
      return value;
    }
  }
}