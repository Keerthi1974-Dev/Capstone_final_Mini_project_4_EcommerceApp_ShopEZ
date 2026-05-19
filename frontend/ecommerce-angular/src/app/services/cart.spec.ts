import { TestBed } from '@angular/core/testing';
import { CartService } from './cart';
import { Product } from '../models/product';

describe('CartService', () => {

  let service: CartService;

  const mockProduct: Product = {
    productId: 1,
    name: 'Test Product',
    description: 'Test Description',
    price: 500,
    stock: 10,
    category: 'Test',
    imageUrl: 'http://test.com/image.jpg'
  };

  const mockProduct2: Product = {
    productId: 2,
    name: 'Test Product 2',
    description: 'Test Description 2',
    price: 300,
    stock: 5,
    category: 'Test',
    imageUrl: 'http://test.com/image2.jpg'
  };

  beforeEach(() => {

    TestBed.configureTestingModule({
      providers: [CartService]
    });

    service = TestBed.inject(CartService);
    service.clearCart();
  });

  // ─── Creation ────────────────────────────────────────────────────────────────

  it('should be created', () => {

    expect(service).toBeTruthy();
  });

  // ─── addToCart ────────────────────────────────────────────────────────────────

  it('should add product to cart', () => {

    service.addToCart(mockProduct);

    expect(service.getItems().length).toBe(1);
    expect(service.getItems()[0].product.productId).toBe(1);
    expect(service.getItems()[0].quantity).toBe(1);
  });

  it('should increase quantity when same product added again', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct);

    expect(service.getItems().length).toBe(1);
    expect(service.getItems()[0].quantity).toBe(2);
  });

  it('should add multiple different products to cart', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);

    expect(service.getItems().length).toBe(2);
  });

  // ─── removeFromCart ───────────────────────────────────────────────────────────

  it('should remove product from cart', () => {

    service.addToCart(mockProduct);
    service.removeFromCart(1);

    expect(service.getItems().length).toBe(0);
  });

  it('should only remove the correct product', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);
    service.removeFromCart(1);

    expect(service.getItems().length).toBe(1);
    expect(service.getItems()[0].product.productId).toBe(2);
  });

  it('should do nothing when removing product not in cart', () => {

    service.addToCart(mockProduct);
    service.removeFromCart(999);

    expect(service.getItems().length).toBe(1);
  });

  // ─── updateQuantity ───────────────────────────────────────────────────────────

  it('should update quantity of product in cart', () => {

    service.addToCart(mockProduct);
    service.updateQuantity(1, 5);

    expect(service.getItems()[0].quantity).toBe(5);
  });

  it('should remove product when quantity updated to 0', () => {

    service.addToCart(mockProduct);
    service.updateQuantity(1, 0);

    expect(service.getItems().length).toBe(0);
  });

  it('should remove product when quantity updated to negative value', () => {

    service.addToCart(mockProduct);
    service.updateQuantity(1, -1);

    expect(service.getItems().length).toBe(0);
  });

  // ─── clearCart ────────────────────────────────────────────────────────────────

  it('should clear all items from cart', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);
    service.clearCart();

    expect(service.getItems().length).toBe(0);
  });

  // ─── getTotal ─────────────────────────────────────────────────────────────────

  it('should return 0 total when cart is empty', () => {

    expect(service.getTotal()).toBe(0);
  });

  it('should calculate total correctly', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);

    expect(service.getTotal()).toBe(800);
  });

  it('should calculate total with quantities', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct);

    expect(service.getTotal()).toBe(1000);
  });

  it('should recalculate total after removing a product', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);
    service.removeFromCart(1);

    expect(service.getTotal()).toBe(300);
  });

  // ─── getCount ─────────────────────────────────────────────────────────────────

  it('should return 0 count when cart is empty', () => {

    expect(service.getCount()).toBe(0);
  });

  it('should return correct item count', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);

    expect(service.getCount()).toBe(3);
  });

  // ─── cart$ observable — Promise-based (Vitest does NOT support done()) ────────

  it('should emit cart items via cart$ observable', () => {

    return new Promise<void>((resolve) => {

      service.cart$.subscribe((items) => {

        if (items.length > 0) {
          expect(items[0].product.productId).toBe(1);
          resolve();
        }
      });

      service.addToCart(mockProduct);
    });
  });

  it('should emit empty array via cart$ after clearCart', () => {

    service.addToCart(mockProduct);

    return new Promise<void>((resolve) => {

      service.cart$.subscribe((items) => {

        if (items.length === 0) {
          expect(items.length).toBe(0);
          resolve();
        }
      });

      service.clearCart();
    });
  });

  it('should emit updated items via cart$ after removeFromCart', () => {

    service.addToCart(mockProduct);
    service.addToCart(mockProduct2);

    return new Promise<void>((resolve) => {

      service.cart$.subscribe((items) => {

        if (items.length === 1) {
          expect(items[0].product.productId).toBe(2);
          resolve();
        }
      });

      service.removeFromCart(1);
    });
  });

  it('should emit updated quantity via cart$ after updateQuantity', () => {

    service.addToCart(mockProduct);

    return new Promise<void>((resolve) => {

      service.cart$.subscribe((items) => {

        if (items.length > 0 && items[0].quantity === 5) {
          expect(items[0].quantity).toBe(5);
          resolve();
        }
      });

      service.updateQuantity(1, 5);
    });
  });

});