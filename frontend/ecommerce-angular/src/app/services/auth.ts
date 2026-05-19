import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { LoginDTO, RegisterDTO, AuthResponse } from '../models/user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = `${environment.apiUrl}/api/auth`;

  constructor(private http: HttpClient, private router: Router) {}

  register(dto: RegisterDTO): Observable<string> {
    return this.http.post(`${this.baseUrl}/register`, dto, {
      responseType: 'text'
    });
  }

  login(dto: LoginDTO): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/login`, dto).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);
        localStorage.setItem('user', JSON.stringify(response.user));
      })
    );
  }

  refreshToken(): Observable<any> {

    const refreshToken = localStorage.getItem('refreshToken');

    if (!refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token found'));
    }

    return this.http.post<any>(
      `${this.baseUrl}/refresh-token`,
      {
        refreshToken
      }
    ).pipe(
      tap(response => {

        console.log('Refresh response: - auth.ts:48', response);

        localStorage.setItem('token', response.token);
        localStorage.setItem('refreshToken', response.refreshToken);

        if (response.user) {
          localStorage.setItem('user', JSON.stringify(response.user));
        }
      }),
      catchError(error => {

        console.error('Refresh token failed: - auth.ts:59', error);

        this.logout();

        return throwError(() => error);
      })
    );
  }

  logout(): void {

    const refreshToken = localStorage.getItem('refreshToken') ?? '';

    this.http.post(
      `${this.baseUrl}/logout`,
      {
        refreshToken
      }
    ).subscribe({ error: () => {} });

    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');

    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUser(): any | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    return this.getUser()?.role === 'Admin';
  }
}