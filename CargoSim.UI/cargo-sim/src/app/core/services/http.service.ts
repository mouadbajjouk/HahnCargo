import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Endpoint } from '../enums/endpoint';

@Injectable({
  providedIn: 'root',
})
export class HttpService {
  constructor() {}

  httpClient = inject(HttpClient);

  get<T>(endpoint: Endpoint, options?: any): Observable<T> {
    return this.httpClient.get<T>(
      'https://localhost:59067/api/' + endpoint,
      options
    ) as Observable<T>;
  }

  post<T>(endpoint: Endpoint, data: any, options?: any): Observable<T> {
    return this.httpClient.post<T>(
      'https://localhost:59067/api/' + endpoint,
      data,
      options
    ) as Observable<T>;
  }
}
