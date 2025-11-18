import { InjectionToken } from '@angular/core';
import { ApiConfig } from '../core/services/api.service';
import { StorageConfig } from './storage-config';

export const environment = {
  production: false,
  api: {
    baseUrl: 'http://localhost:3000/api',
    timeout: 30000,
    retryAttempts: 3
  } as ApiConfig,
  storage: {
    encryptionKey: 'prod-secure-key-change-in-production',
    prefix: 'app_'
  } as StorageConfig
};

export const API_CONFIG = new InjectionToken<ApiConfig>('api.config');
export const STORAGE_CONFIG = new InjectionToken<StorageConfig>('storage.config');