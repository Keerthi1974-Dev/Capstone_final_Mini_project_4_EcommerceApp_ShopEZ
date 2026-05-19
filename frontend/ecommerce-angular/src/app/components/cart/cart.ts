import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartService, CartItem } from '../../services/cart';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.html'
})
export class CartComponent {
  items: CartItem[] = [];

  constructor(public cartService: CartService, public router: Router) {
    this.cartService.cart$.subscribe(items => this.items = items);
  }

  increment(productId: number): void {
    const item = this.items.find(i => i.product.productId === productId);
    if (item) this.cartService.updateQuantity(productId, item.quantity + 1);
  }

  decrement(productId: number): void {
    const item = this.items.find(i => i.product.productId === productId);
    if (item && item.quantity > 1) this.cartService.updateQuantity(productId, item.quantity - 1);
  }
}