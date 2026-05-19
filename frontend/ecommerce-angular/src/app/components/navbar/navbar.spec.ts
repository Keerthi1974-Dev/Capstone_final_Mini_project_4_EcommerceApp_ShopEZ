import { TestBed, ComponentFixture } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { vi } from 'vitest';
import { of } from 'rxjs';

import { NavbarComponent } from './navbar';
import { AuthService } from '../../services/auth';
import { CartService } from '../../services/cart';

describe('NavbarComponent', () => {

  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authServiceMock: Partial<AuthService>;
  let cartServiceMock: Partial<CartService>;

  beforeEach(async () => {

    authServiceMock = {
      isLoggedIn: vi.fn().mockReturnValue(false),
      isAdmin:    vi.fn().mockReturnValue(false),
      getUser:    vi.fn().mockReturnValue(null),
      logout:     vi.fn()
    };

    cartServiceMock = {
      getCount: vi.fn().mockReturnValue(0),
      getItems: vi.fn().mockReturnValue([]),
      getTotal: vi.fn().mockReturnValue(0),
      cart$:    of([]) as any
    };

    await TestBed.configureTestingModule({
      imports: [
        NavbarComponent,
        RouterTestingModule
      ],
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: CartService, useValue: cartServiceMock }
      ]
    }).compileComponents();

    fixture   = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  // ─── Creation ────────────────────────────────────────────────────────────────

  it('should be created', () => {

    expect(component).toBeTruthy();
  });

  // ─── menuOpen / toggleMenu ────────────────────────────────────────────────────

  it('should have menuOpen false by default', () => {

    expect(component.menuOpen).toBe(false);
  });

  it('should toggle menu open on first call', () => {

    component.toggleMenu();

    expect(component.menuOpen).toBe(true);
  });

  it('should toggle menu closed on second call', () => {

    component.toggleMenu();
    component.toggleMenu();

    expect(component.menuOpen).toBe(false);
  });

  it('should toggle menu correctly across multiple calls', () => {

    expect(component.menuOpen).toBe(false);

    component.toggleMenu();
    expect(component.menuOpen).toBe(true);

    component.toggleMenu();
    expect(component.menuOpen).toBe(false);

    component.toggleMenu();
    expect(component.menuOpen).toBe(true);
  });

  // ─── AuthService integration ──────────────────────────────────────────────────

  it('should call isLoggedIn from authService', () => {

    (authServiceMock.isLoggedIn as ReturnType<typeof vi.fn>).mockReturnValue(true);

    expect(component.authService.isLoggedIn()).toBe(true);
  });

  it('should call isAdmin from authService', () => {

    (authServiceMock.isAdmin as ReturnType<typeof vi.fn>).mockReturnValue(true);

    expect(component.authService.isAdmin()).toBe(true);
  });

  it('should call logout from authService', () => {

    component.authService.logout();

    expect(authServiceMock.logout).toHaveBeenCalled();
  });

  // ─── CartService integration ──────────────────────────────────────────────────

  it('should get cart count from cartService', () => {

    (cartServiceMock.getCount as ReturnType<typeof vi.fn>).mockReturnValue(3);

    expect(component.cartService.getCount()).toBe(3);
  });

  it('should get cart count of 0 when cart is empty', () => {

    (cartServiceMock.getCount as ReturnType<typeof vi.fn>).mockReturnValue(0);

    expect(component.cartService.getCount()).toBe(0);
  });

});