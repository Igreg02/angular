import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment/environments';
import { Interest } from '../models/interest.models';
import { InterestCreateRequest } from '../models/interest-create.model';

@Injectable({
  providedIn: 'root',
})
export class InterestService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/interests`;

  getAll(): Observable<Interest[]>
  {
    return this.http.get<Interest[]>(this.baseUrl);
  }

  getbyId(id:number): Observable<Interest>
  {
    return this.http.get<Interest>(`${this.baseUrl}/${id}`);
  }

  create(payload:InterestCreateRequest): Observable<Interest>
  {
    return this.http.post<Interest>(this.baseUrl, payload);
  }

  update(id:number, payload: InterestCreateRequest): Observable<Interest>
  {
    return this.http.put<Interest>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id:number): Observable<void>
  {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
