import { TestBed, ComponentFixture } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { of, throwError } from 'rxjs';

import { LoginComponent } from './login';
import { AuthService } from '../../services/auth';

// Dummy stub components for routes
import { Component } from '@angular/core';
@Component({ template: '', standalone: true })
class StubComponent {}

describe('LoginComponent', () => {

  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceMock: Partial<AuthService>;
  let router: Router;

  beforeEach(async () => {

    authServiceMock = {
      isLoggedIn: vi.fn().mockReturnValue(false),
      login:      vi.fn().mockReturnValue(of({}))
    };

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        FormsModule,
        CommonModule,
        // ✅ Provide stub routes so NG04002 doesn't fire
        RouterTestingModule.withRoutes([
          { path: 'products', component: StubComponent },
          { path: 'login',    component: StubComponent },
          { path: '',         redirectTo: 'products', pathMatch: 'full' }
        ])
      ],
      providers: [
        { provide: AuthService, useValue: authServiceMock }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);

    fixture   = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ─── Creation ────────────────────────────────────────────────────────────────

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // ─── Default state ────────────────────────────────────────────────────────────

  it('should have empty email by default', () => {
    expect(component.email).toBe('');
  });

  it('should have empty password by default', () => {
    expect(component.password).toBe('');
  });

  it('should have empty error by default', () => {
    expect(component.error).toBe('');
  });

  it('should have loading false by default', () => {
    expect(component.loading).toBe(false);
  });

  it('should have showPassword false by default', () => {
    expect(component.showPassword).toBe(false);
  });

  // ─── Constructor redirect ─────────────────────────────────────────────────────

  it('should redirect to /products if already logged in', async () => {

    (authServiceMock.isLoggedIn as ReturnType<typeof vi.fn>).mockReturnValue(true);

    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture   = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;

    expect(navigateSpy).toHaveBeenCalledWith(['/products']);
  });

  it('should not redirect if user is not logged in', () => {

    (authServiceMock.isLoggedIn as ReturnType<typeof vi.fn>).mockReturnValue(false);

    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(LoginComponent);

    expect(navigateSpy).not.toHaveBeenCalled();
  });

  // ─── login() — invalid form ───────────────────────────────────────────────────

  it('should not call authService.login if form is invalid', () => {

    const mockForm: any = { invalid: true };
    component.login(mockForm);
    expect(authServiceMock.login).not.toHaveBeenCalled();
  });

  it('should not change loading state if form is invalid', () => {

    const mockForm: any = { invalid: true };
    component.login(mockForm);
    expect(component.loading).toBe(false);
  });

  // ─── login() — success ────────────────────────────────────────────────────────

  it('should call authService.login with correct credentials', () => {

    component.email    = 'john@test.com';
    component.password = '123456';

    const mockForm: any = { invalid: false };
    component.login(mockForm);

    expect(authServiceMock.login).toHaveBeenCalledWith({
      email: 'john@test.com',
      password: '123456'
    });
  });

  it('should set loading to true while logging in', () => {

    (authServiceMock.login as ReturnType<typeof vi.fn>).mockImplementation(() => {
      expect(component.loading).toBe(true);
      return of({});
    });

    const mockForm: any = { invalid: false };
    component.login(mockForm);
  });

  it('should set loading to false after successful login', () => {

    const mockForm: any = { invalid: false };
    component.login(mockForm);
    expect(component.loading).toBe(false);
  });

  it('should navigate to /products on successful login', () => {

    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const mockForm: any = { invalid: false };
    component.login(mockForm);

    expect(navigateSpy).toHaveBeenCalledWith(['/products']);
  });

  it('should clear error message before attempting login', () => {

    component.error = 'Previous error';

    const mockForm: any = { invalid: false };
    component.login(mockForm);

    expect(component.error).toBe('');
  });

  // ─── login() — error ──────────────────────────────────────────────────────────

  it('should set error message on failed login', () => {

    (authServiceMock.login as ReturnType<typeof vi.fn>)
      .mockReturnValue(throwError(() => new Error('Unauthorized')));

    const mockForm: any = { invalid: false };
    component.login(mockForm);

    expect(component.error).toBe('Invalid email or password. Please try again.');
  });

  it('should set loading to false after failed login', () => {

    (authServiceMock.login as ReturnType<typeof vi.fn>)
      .mockReturnValue(throwError(() => new Error('Unauthorized')));

    const mockForm: any = { invalid: false };
    component.login(mockForm);

    expect(component.loading).toBe(false);
  });

  it('should not navigate on failed login', () => {

    (authServiceMock.login as ReturnType<typeof vi.fn>)
      .mockReturnValue(throwError(() => new Error('Unauthorized')));

    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const mockForm: any = { invalid: false };
    component.login(mockForm);

    expect(navigateSpy).not.toHaveBeenCalled();
  });

});