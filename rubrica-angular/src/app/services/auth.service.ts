import {Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { retry, tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { enviroment } from '../../enviroments/enviroment';
import { AuthResponse } from '../models/auth-response.model';
import { LoginRequest } from '../models/login.model';
import { RegisterRequest } from '../models/register.model';
import { SessionUser } from '../models/session-user.model';

Injectable({ providedIn: 'root'})
export class AuthService 
{
    private readonly http = inject(HttpClient);
    private readonly router = inject(Router);
    private readonly storageKey = 'rubrica_auth';

    readonly currentUser = signal<SessionUser | null>(this.readStoredUser());

    login(payload: LoginRequest): Observable<AuthResponse>
    {
        return this.http
        .post<AuthResponse>(`${enviroment.apiBaseUrl}/Auth/login`, payload)
        .pipe(tap((response) => this.setSession(response)));
    }

    register(payload: RegisterRequest): Observable<{ message : string }>
    {
        return this.http.post<{ message : string }>(`${enviroment.apiBaseUrl}/Auth/register`, payload);
    }

    logout(): void
    {
        localStorage.removeItem(this.storageKey);
        this.currentUser.set(null);
        void this.router.navigate(['/login']);
    }

    isAuthenticated(): boolean
    {
        return this.currentUser() != null;
    }

    hasAnyRole(roles: string []): boolean
    {
    const role = this.currentUser()?.role ?? '';
    return roles.includes(role);
    }

    hasRole(role:string): boolean
    {
        return this.currentUser()?.role == role;
    }

    getToken(): string | null 
    {
        return this.currentUser()?.token ?? null;
    }
    
    private setSession(response: AuthResponse): void
    {
        const sessionUser : SessionUser = 
        {
            token : response.token,
            userId : response.userId,
            email : response.email,
            nomeCompleto : response.nomeCompleto,
            role: response.role
        };
    
    localStorage.setItem(this.storageKey, JSON.stringify(sessionUser));
    this.currentUser.set(sessionUser);
    }

    private readStoredUser(): SessionUser | null 
    {
        const raw = localStorage.getItem(this.storageKey);

        if (!raw) {return null;}

        try 
        {
            return JSON.parse(raw) as SessionUser;
        }
        catch
        {
            localStorage.removeItem(this.storageKey);
            return null;
        }
    }
}