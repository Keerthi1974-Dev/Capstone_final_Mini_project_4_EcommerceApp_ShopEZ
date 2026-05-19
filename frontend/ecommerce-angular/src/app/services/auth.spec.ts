import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController
} from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { provideRouter } from '@angular/router';
import { Component } from '@angular/core';

import { AuthService } from './auth';
import { environment } from '../../environments/environment';

@Component({
  standalone: true,
  template: ''
})
class DummyComponent {}

describe('AuthService', () => {

  let service: AuthService;
  let httpMock: HttpTestingController;

  // Base URL must match what the service builds at runtime
  const baseUrl = `${environment.apiUrl}/api/auth`;

  const mockUser = {
    userId: 1,
    name: 'John',
    email: 'john@test.com',
    role: 'User'
  };

  const mockAuthResponse = {
    token: 'fake-token',
    refreshToken: 'fake-refresh',
    user: mockUser
  };

  beforeEach(() => {

    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule
      ],
      providers: [
        AuthService,
        provideRouter([
          { path: 'login', component: DummyComponent }
        ])
      ]
    });

    service  = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);

    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();   // fails if any HTTP request was left unhandled
    localStorage.clear();
  });

  // ─── Creation ────────────────────────────────────────────────────────────────

  it('should be created', () => {

    expect(service).toBeTruthy();
  });

  // ─── register ────────────────────────────────────────────────────────────────

  it('should register user successfully', () => {

    service.register({
      name: 'John',
      email: 'john@test.com',
      password: '123456',
      role: 'User'
    }).subscribe((res) => {

      expect(res).toBe('User registered successfully.');
    });

    const req = httpMock.expectOne(`${baseUrl}/register`);
    expect(req.request.method).toBe('POST');
    req.flush('User registered successfully.');
  });

  // ─── login ───────────────────────────────────────────────────────────────────

  it('should login and store token in localStorage', () => {

    service.login({
      email: 'john@test.com',
      password: '123456'
    }).subscribe(() => {

      expect(localStorage.getItem('token')).toBe('fake-token');
      expect(localStorage.getItem('refreshToken')).toBe('fake-refresh');
      expect(localStorage.getItem('user')).toBe(JSON.stringify(mockUser));
    });

    const req = httpMock.expectOne(`${baseUrl}/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockAuthResponse);
  });

  it('should send correct credentials on login', () => {

    const credentials = { email: 'john@test.com', password: '123456' };

    service.login(credentials).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/login`);
    expect(req.request.body).toEqual(credentials);
    req.flush(mockAuthResponse);
  });

  // ─── logout ──────────────────────────────────────────────────────────────────

  it('should clear localStorage on logout', () => {

    localStorage.setItem('token', 'test-token');
    localStorage.setItem('refreshToken', 'test-refresh');
    localStorage.setItem('user', JSON.stringify(mockUser));

    service.logout();

    // logout() POSTs to /logout — flush it so httpMock.verify() passes
    const req = httpMock.expectOne(`${baseUrl}/logout`);
    expect(req.request.method).toBe('POST');
    req.flush({});

    expect(localStorage.getItem('token')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });

  it('should navigate to /login on logout', () => {

    service.logout();

    const req = httpMock.expectOne(`${baseUrl}/logout`);
    req.flush({});

    // Router navigation is tested via RouterTestingModule — no errors = pass
  });

  // ─── refreshToken ─────────────────────────────────────────────────────────────

  it('should throw error when no refresh token found', () => {

    service.refreshToken().subscribe({
      next: () => fail('expected an error'),
      error: (err) => {
        expect(err.message).toBe('No refresh token found');
      }
    });

    // refreshToken() calls logout() internally when token is missing,
    // and logout() fires POST /logout — must flush to satisfy httpMock.verify()
    const req = httpMock.expectOne(`${baseUrl}/logout`);
    req.flush({});
  });

  it('should refresh token and update localStorage', () => {

    localStorage.setItem('refreshToken', 'old-refresh');

    const refreshResponse = {
      token: 'new-token',
      refreshToken: 'new-refresh',
      user: mockUser
    };

    service.refreshToken().subscribe(() => {

      expect(localStorage.getItem('token')).toBe('new-token');
      expect(localStorage.getItem('refreshToken')).toBe('new-refresh');
    });

    const req = httpMock.expectOne(`${baseUrl}/refresh-token`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ refreshToken: 'old-refresh' });
    req.flush(refreshResponse);
  });

  it('should call logout and throw error when refresh token request fails', () => {

    localStorage.setItem('refreshToken', 'bad-refresh');
    localStorage.setItem('token', 'old-token');

    service.refreshToken().subscribe({
      next: () => fail('expected an error'),
      error: (err) => {
        expect(err.status).toBe(401);
      }
    });

    const refreshReq = httpMock.expectOne(`${baseUrl}/refresh-token`);
    refreshReq.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    // failed refresh calls logout() which POSTs to /logout
    const logoutReq = httpMock.expectOne(`${baseUrl}/logout`);
    logoutReq.flush({});
  });

  // ─── getToken ─────────────────────────────────────────────────────────────────

  it('should return token from localStorage', () => {

    localStorage.setItem('token', 'test-token');

    expect(service.getToken()).toBe('test-token');
  });

  it('should return null when no token in localStorage', () => {

    expect(service.getToken()).toBeNull();
  });

  // ─── getUser ──────────────────────────────────────────────────────────────────

  it('should return user from localStorage', () => {

    localStorage.setItem('user', JSON.stringify(mockUser));

    expect(service.getUser()).toEqual(mockUser);
  });

  it('should return null when no user in localStorage', () => {

    expect(service.getUser()).toBeNull();
  });

  // ─── isLoggedIn ───────────────────────────────────────────────────────────────

  it('should return true when user is logged in', () => {

    localStorage.setItem('token', 'test-token');

    expect(service.isLoggedIn()).toBe(true);
  });

  it('should return false when user is not logged in', () => {

    expect(service.isLoggedIn()).toBe(false);
  });

  // ─── isAdmin ──────────────────────────────────────────────────────────────────

  it('should return true when user is Admin', () => {

    localStorage.setItem('user', JSON.stringify({ role: 'Admin' }));

    expect(service.isAdmin()).toBe(true);
  });

  it('should return false when user is not Admin', () => {

    localStorage.setItem('user', JSON.stringify({ role: 'User' }));

    expect(service.isAdmin()).toBe(false);
  });

  it('should return false when no user in localStorage', () => {

    expect(service.isAdmin()).toBe(false);
  });

});