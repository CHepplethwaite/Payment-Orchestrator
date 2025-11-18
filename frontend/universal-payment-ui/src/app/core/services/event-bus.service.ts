import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';

export interface EventBusEvent {
  type: string;
  payload?: any;
}

@Injectable({
  providedIn: 'root'
})
export class EventBusService {
  private eventSubject = new Subject<EventBusEvent>();

  emit(event: EventBusEvent): void {
    this.eventSubject.next(event);
  }

  on(eventType: string): Observable<EventBusEvent> {
    return this.eventSubject.asObservable().pipe(
      filter(event => event.type === eventType)
    );
  }

  // Common event types
  static readonly SHOW_ERROR = 'SHOW_ERROR';
  static readonly SHOW_SUCCESS = 'SHOW_SUCCESS';
  static readonly HTTP_ERROR = 'HTTP_ERROR';
  static readonly AUTH_EXPIRED = 'AUTH_EXPIRED';
  static readonly NETWORK_ERROR = 'NETWORK_ERROR';
}