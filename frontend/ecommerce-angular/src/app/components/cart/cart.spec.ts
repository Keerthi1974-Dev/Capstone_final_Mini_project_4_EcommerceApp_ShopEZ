import { TestBed, ComponentFixture } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { CartComponent } from './cart';
import { CartService } from '../../services/cart';
import { vi } from 'vitest';
import { of } from 'rxjs';

describe('CartComponent', () => {
  let component: CartComponent;
  let fixture: ComponentFixture<CartComponent>;
  let cartServiceSpy: any;

  beforeEach(async () => {
    // ✅ Replaced jasmine.createSpyObj with vi.fn()
    const spy = {
      getItems:       vi.fn().mockReturnValue([]),
      getTotal:       vi.fn().mockReturnValue(0),
      updateQuantity: vi.fn(),
      clearCart:      vi.fn(),
      cart$:          of([])
    };

    await TestBed.configureTestingModule({
      imports: [CartComponent, RouterTestingModule],
      providers: [{ provide: CartService, useValue: spy }]
    }).compileComponents();

    cartServiceSpy = TestBed.inject(CartService);
    fixture = TestBed.createComponent(CartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  it('should increment item quantity', () => {
    component.items = [{
      product: { productId: 1, name: 'Test', price: 500, stock: 10, description: '', category: '', imageUrl: '' },
      quantity: 1
    }];
    component.increment(1);
    expect(cartServiceSpy.updateQuantity).toHaveBeenCalledWith(1, 2);
  });

  it('should decrement item quantity', () => {
    component.items = [{
      product: { productId: 1, name: 'Test', price: 500, stock: 10, description: '', category: '', imageUrl: '' },
      quantity: 3
    }];
    component.decrement(1);
    expect(cartServiceSpy.updateQuantity).toHaveBeenCalledWith(1, 2);
  });

  it('should not decrement below 1', () => {
    component.items = [{
      product: { productId: 1, name: 'Test', price: 500, stock: 10, description: '', category: '', imageUrl: '' },
      quantity: 1
    }];
    component.decrement(1);
    expect(cartServiceSpy.updateQuantity).not.toHaveBeenCalled();
  });
});