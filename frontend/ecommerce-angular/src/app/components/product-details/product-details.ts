import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product';
import { CartService } from '../../services/cart';
import { Product } from '../../models/product';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-details.html'
})
export class ProductDetailsComponent implements OnInit {

  product: Product | null = null;
  quantity = 1;
  added = false;
  loading = true;
  error = false;  

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private productService: ProductService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
  this.route.paramMap.subscribe(params => {
    const id = Number(params.get('id'));

    console.log("Route changed ID: - product-details.ts:33", id);

    // RESET STATE 
    this.product = null;
    this.loading = true;

    this.productService.getById(id).subscribe({
      next: (p) => {
        console.log("API DATA: - product-details.ts:41", p);

        // FORCE CHANGE DETECTION
        setTimeout(() => {
          this.product = p;
          this.loading = false;
        }, 0);
      },
      error: (err) => {
        console.log("ERROR: - product-details.ts:50", err);
        this.loading = false;
      }
    });
  });
}

  goBack(): void {
    this.router.navigate(['/products']);
  }

  increment(): void {
    if (this.product && this.quantity < this.product.stock) {
      this.quantity++;
    }
  }

  decrement(): void {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  addToCart(): void {
    if (!this.product) return;

    for (let i = 0; i < this.quantity; i++) {
      this.cartService.addToCart(this.product);
    }

    this.added = true;
    setTimeout(() => this.added = false, 2000);
  }
}