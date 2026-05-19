import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest,
  HttpHandler, HttpEvent, HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, switchMap, filter, take } from 'rxjs/operators';
import { AuthService } from '../services/auth';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // Attach token to every request
    const token = this.authService.getToken();
    const authReq = token ? req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    }) : req;

    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {

        // Never try to refresh for auth calls — prevents infinite loop
        const isAuthCall = req.url.includes('/api/auth');
        if (error.status === 401 && !isAuthCall) {
          return this.handle401(req, next);
        }

        return throwError(() => error);
      })
    );
  }

  private handle401(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.isRefreshing) {
      // Wait for the ongoing refresh to finish, then retry
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(token => {
          return next.handle(req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
          }));
        })
      );
    }

    this.isRefreshing = true;
    this.refreshTokenSubject.next(null);

    return this.authService.refreshToken().pipe(
      switchMap(response => {
        this.isRefreshing = false;
        this.refreshTokenSubject.next(response.token);
        // Retry original request with new token
        return next.handle(req.clone({
          setHeaders: { Authorization: `Bearer ${response.token}` }
        }));
      }),
      catchError(err => {
        this.isRefreshing = false;
        if (err.status === 401) {
          this.authService.logout();
        }
        return throwError(() => err);
      })
    );
  }
}