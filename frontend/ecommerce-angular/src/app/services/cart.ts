import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../models/product';

export interface CartItem {
  product: Product;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private items: CartItem[] = [];
  private cartSubject = new BehaviorSubject<CartItem[]>([]);

  cart$ = this.cartSubject.asObservable();

  addToCart(product: Product): void {
    const existing = this.items.find(i => i.product.productId === product.productId);
    if (existing) {
      existing.quantity++;
    } else {
      this.items.push({ product, quantity: 1 });
    }
    this.cartSubject.next([...this.items]);
  }

  removeFromCart(productId: number): void {
    this.items = this.items.filter(i => i.product.productId !== productId);
    this.cartSubject.next([...this.items]);
  }

  updateQuantity(productId: number, quantity: number): void {
    const item = this.items.find(i => i.product.productId === productId);
    if (item) {
      item.quantity = quantity;
      if (item.quantity <= 0) this.removeFromCart(productId);
    }
    this.cartSubject.next([...this.items]);
  }

  clearCart(): void {
    this.items = [];
    this.cartSubject.next([]);
  }

  getItems(): CartItem[] {
    return this.items;
  }

  getTotal(): number {
    return this.items.reduce((sum, i) => sum + i.product.price * i.quantity, 0);
  }

  getCount(): number {
    return this.items.reduce((sum, i) => sum + i.quantity, 0);
  }
}