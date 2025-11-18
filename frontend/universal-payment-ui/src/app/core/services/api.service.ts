import { Injectable, InjectionToken, Inject } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoggerService } from './logger.service';

export const API_CONFIG = new InjectionToken<ApiConfig>('api.config');

export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  retryAttempts: number;
}

export interface ApiOptions {
  params?: { [key: string]: any };
  headers?: { [key: string]: string };
  responseType?: 'json' | 'text' | 'blob' | 'arraybuffer';
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor(
    private http: HttpClient,
    private logger: LoggerService,
    @Inject(API_CONFIG) private config: ApiConfig
  ) {}

  get<T>(url: string, options: ApiOptions = {}): Observable<T> {
    this.logger.debug(`API GET: ${url}`, options);
    
    const httpOptions = this.buildHttpOptions(options);
    return this.http.get<T>(this.buildUrl(url), httpOptions);
  }

  post<T>(url: string, body: any, options: ApiOptions = {}): Observable<T> {
    this.logger.debug(`API POST: ${url}`, { body, options });
    
    const httpOptions = this.buildHttpOptions(options);
    return this.http.post<T>(this.buildUrl(url), body, httpOptions);
  }

  put<T>(url: string, body: any, options: ApiOptions = {}): Observable<T> {
    this.logger.debug(`API PUT: ${url}`, { body, options });
    
    const httpOptions = this.buildHttpOptions(options);
    return this.http.put<T>(this.buildUrl(url), body, httpOptions);
  }

  patch<T>(url: string, body: any, options: ApiOptions = {}): Observable<T> {
    this.logger.debug(`API PATCH: ${url}`, { body, options });
    
    const httpOptions = this.buildHttpOptions(options);
    return this.http.patch<T>(this.buildUrl(url), body, httpOptions);
  }

  delete<T>(url: string, options: ApiOptions = {}): Observable<T> {
    this.logger.debug(`API DELETE: ${url}`, options);
    
    const httpOptions = this.buildHttpOptions(options);
    return this.http.delete<T>(this.buildUrl(url), httpOptions);
  }

  private buildUrl(url: string): string {
    if (url.startsWith('http')) {
      return url;
    }
    return `${this.config.baseUrl}${url}`;
  }

  private buildHttpOptions(options: ApiOptions): {
    headers?: HttpHeaders;
    params?: HttpParams;
    responseType?: any;
  } {
    let httpParams = new HttpParams();
    if (options.params) {
      Object.keys(options.params).forEach(key => {
        const value = options.params![key];
        if (value !== null && value !== undefined) {
          httpParams = httpParams.set(key, value.toString());
        }
      });
    }

    let httpHeaders = new HttpHeaders();
    if (options.headers) {
      Object.keys(options.headers).forEach(key => {
        httpHeaders = httpHeaders.set(key, options.headers![key]);
      });
    }

    return {
      headers: httpHeaders,
      params: httpParams,
      responseType: options.responseType || 'json'
    };
  }
}