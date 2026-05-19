import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService, CartItem } from '../../services/cart';
import { OrderService } from '../../services/order';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class CheckoutComponent {

  items: CartItem[] = [];
  total: number = 0;
  message = '';
  error = '';
  loading = false;

  // ── Step control ──────────────────────────────────────
  currentStep = 1; // 1 = Address, 2 = Payment, 3 = Review

  // ── Address form ──────────────────────────────────────
  address = {
    fullName:  '',
    phone:     '',
    pincode:   '',
    street:    '',
    city:      '',
    state:     '',
  };
  addressError = '';

  // ── Payment ───────────────────────────────────────────
  selectedPayment = '';  // 'cod' | 'upi' | 'card' | 'netbanking'
  upiId = '';
  cardNumber = '';
  cardName = '';
  cardExpiry = '';
  cardCvv = '';
  selectedBank = '';
  paymentError = '';

  banks = ['SBI', 'HDFC Bank', 'ICICI Bank', 'Axis Bank', 'Kotak Bank', 'PNB', 'Bank of Baroda'];

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    public router: Router
  ) {
    this.items = this.cartService.getItems();
    this.total = this.cartService.getTotal();
    if (this.items.length === 0) this.router.navigate(['/cart']);
  }

  // ── Step 1: Validate address ──────────
  proceedToPayment(): void {
    this.addressError = '';
    const { fullName, phone, pincode, street, city, state } = this.address;

    if (!fullName.trim())          { this.addressError = 'Full name is required'; return; }
    if (!/^\d{10}$/.test(phone))   { this.addressError = 'Enter a valid 10-digit phone number'; return; }
    if (!/^\d{6}$/.test(pincode))  { this.addressError = 'Enter a valid 6-digit pincode'; return; }
    if (!street.trim())            { this.addressError = 'Street address is required'; return; }
    if (!city.trim())              { this.addressError = 'City is required'; return; }
    if (!state.trim())             { this.addressError = 'State is required'; return; }

    this.currentStep = 2;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // ── Step 2: Validate payment ───
  proceedToReview(): void {
    this.paymentError = '';

    if (!this.selectedPayment) {
      this.paymentError = 'Please select a payment method'; return;
    }
    if (this.selectedPayment === 'upi' && !this.upiId.trim()) {
      this.paymentError = 'Please enter your UPI ID'; return;
    }
    if (this.selectedPayment === 'upi' && !this.upiId.includes('@')) {
      this.paymentError = 'Enter a valid UPI ID (e.g. name@upi)'; return;
    }
    if (this.selectedPayment === 'card') {
      if (this.cardNumber.replace(/\s/g,'').length < 16) { this.paymentError = 'Enter a valid 16-digit card number'; return; }
      if (!this.cardName.trim())   { this.paymentError = 'Enter name on card'; return; }
      if (!this.cardExpiry.trim()) { this.paymentError = 'Enter card expiry'; return; }
      if (this.cardCvv.length < 3) { this.paymentError = 'Enter a valid CVV'; return; }
    }
    if (this.selectedPayment === 'netbanking' && !this.selectedBank) {
      this.paymentError = 'Please select a bank'; return;
    }

    this.currentStep = 3;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // ── Format card number with spaces 
  formatCard(e: Event): void {
    const input = e.target as HTMLInputElement;
    let val = input.value.replace(/\D/g, '').slice(0, 16);
    this.cardNumber = val.match(/.{1,4}/g)?.join(' ') || val;
  }

  // Payment label for review 
  getPaymentLabel(): string {
    switch (this.selectedPayment) {
      case 'cod':        return 'Cash on Delivery';
      case 'upi':        return `UPI — ${this.upiId}`;
      case 'card':       return `Card ending in ${this.cardNumber.slice(-4)}`;
      case 'netbanking': return `Net Banking — ${this.selectedBank}`;
      default:           return '';
    }
  }

  // Final: Place Order 
  placeOrder(): void {
    this.error = '';
    this.loading = true;

    const orderDTO = {
      items: this.items.map(i => ({
        productId: i.product.productId,
        quantity:  i.quantity,
        price:     i.product.price,
        imageUrl:  i.product.imageUrl ?? '' 
      }))
    };

    this.orderService.createOrder(orderDTO).subscribe({
      next: (order) => {
        this.loading = false;
        this.cartService.clearCart();
        this.message = `Order #${order.orderId} placed! Total: ₹${order.totalAmount}`;
        setTimeout(() => this.router.navigate(['/orders']), 3000);
      },
      error: (err) => {
        this.loading = false;
        console.error('Order error:', err);
        this.error = `Failed to place order. Status: ${err.status || 'Unknown'}. Please try again.`;
      }
    });
  }
}