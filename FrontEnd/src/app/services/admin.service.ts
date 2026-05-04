import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environment/environments';
import { UserProfile } from '../models/user-profile.model';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/AdminUsers`;

  getAllUsers(): Observable<UserProfile[]>
  {
    return this.http.get<UserProfile[]>(`${this.baseUrl}/users`);
  }

  changeUserRole(email: string, newRole: string): Observable<any>
  {
    return this.http.put(`${this.baseUrl}/change-role`, { email, newRole });
  }
}