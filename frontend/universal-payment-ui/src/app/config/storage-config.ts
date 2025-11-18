import { InjectionToken } from '@angular/core';

export const STORAGE_CONFIG = new InjectionToken<StorageConfig>('storage.config');

export interface StorageConfig {
  encryptionKey: string;
  prefix: string;
}