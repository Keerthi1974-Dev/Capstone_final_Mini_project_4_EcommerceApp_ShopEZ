import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { ProductService } from '../../services/product';
import { CartService } from '../../services/cart';
import { AuthService } from '../../services/auth';
import { Product } from '../../models/product';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-list.html',
  styleUrls: ['./product-list.css']
})
export class ProductListComponent implements OnInit {

  products: Product[] = [];
  filtered: Product[] = [];
  suggestions: Product[] = [];

  search: string = '';
  selectedCategory: string = '';
  activeCategory: string = 'All';
  loading: boolean = true;
  addedId: number | null = null;
  showSuggestions: boolean = false;

  wishlist: Set<number> = new Set();

  categories = [
    { label: 'All', icon: '✨' },
    { label: 'Sarees', icon: '🥻' },
    { label: 'Anarkali', icon: '👗' },
    { label: 'Footwear', icon: '👡' },
    { label: 'Handbags', icon: '👜' },
    { label: 'Jewellery', icon: '💍' },
    { label: 'Skincare', icon: '🧴' },
    { label: 'Summer', icon: '☀️' },
    { label: 'Accessories', icon: '💎' },
  ];

  private categoryKeywords: Record<string, string[]> = {
    'Sarees': ['saree', 'silk', 'chiffon', 'kanjivaram', 'georgette'],
    'Anarkali': ['anarkali', 'suit', 'ethnic dress', 'kurta', 'churidar'],
    'Footwear': ['sandal', 'heel', 'footwear', 'kolhapuri', 'flat', 'shoe', 'slipper'],
    'Handbags': ['bag', 'tote', 'handbag', 'potli', 'clutch', 'purse', 'satchel'],
    'Jewellery': ['choker', 'necklace', 'jewellery', 'kundan', 'bracelet', 'jhumka', 'oxidised', 'pearl'],
    'Skincare': ['sunscreen', 'spf', 'skin', 'cream', 'serum', 'moisturiser', 'tinted'],
    'Summer': ['maxi', 'dress', 'summer', 'floral', 'co-ord', 'cotton', 'tropical'],
    'Accessories': ['hair', 'clip', 'accessories', 'scarf', 'belt', 'watch'],
  };

  constructor(
    private productService: ProductService,
    public cartService: CartService,
    public authService: AuthService,
    public router: Router
  ) {}

  // RESTORED - This was accidentally deleted!
  ngOnInit(): void {
    this.productService.getAll().subscribe({
      next: (data: Product[]) => {
        this.products = data;
        this.filtered = data;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  viewDetails(productId: number): void {
    this.router.navigate(['/products', productId]);
  }

  goToAddProduct(event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/admin/products']);
  }

  toggleWishlist(product: Product): void {
    if (this.wishlist.has(product.productId)) {
      this.wishlist.delete(product.productId);
    } else {
      this.wishlist.add(product.productId);
    }
  }

  onSearch(): void {
    const q = this.search.toLowerCase().trim();
    this.suggestions = q
      ? this.products.filter(p =>
          p.name.toLowerCase().includes(q) ||
          p.description.toLowerCase().includes(q)
        ).slice(0, 6)
      : [];
    this.showSuggestions = this.suggestions.length > 0;
    this.applyFilters();
  }

  pickSuggestion(product: Product): void {
    this.search = product.name;
    this.showSuggestions = false;
    this.applyFilters();
  }

  @HostListener('document:click', ['$event'])
  onDocClick(e: MouseEvent): void {
    const target = e.target as HTMLElement;
    if (!target.closest('.search-wrap')) {
      this.showSuggestions = false;
    }
  }

  filterByCategory(cat: string): void {
    this.activeCategory = cat;
    this.selectedCategory = cat === 'All' ? '' : cat;
    this.search = '';
    this.showSuggestions = false;
    this.applyFilters();
  }

  applyFilters(): void {
    const q = this.search.toLowerCase().trim();
    this.filtered = this.products.filter(p => {
      const nameDesc = (p.name + ' ' + p.description).toLowerCase();
      const matchSearch = !q || nameDesc.includes(q);
      let matchCat = true;
      if (this.selectedCategory && this.selectedCategory !== 'All') {
        const keywords = this.categoryKeywords[this.selectedCategory] || [];
        matchCat = keywords.some(kw => nameDesc.includes(kw));
      }
      return matchSearch && matchCat;
    });
  }

  addToCart(product: Product): void {
    if (product.stock === 0) return;
    this.cartService.addToCart(product);
    this.addedId = product.productId;
    setTimeout(() => this.addedId = null, 1800);
  }

  getStockClass(stock: number): string {
    if (stock === 0) return 'stock-out';
    if (stock <= 5) return 'stock-low';
    return 'stock-ok';
  }

  getStockLabel(stock: number): string {
    if (stock === 0) return 'Out of stock';
    if (stock <= 5) return `Only ${stock} left!`;
    return `${stock} in stock`;
  }

  getDelay(i: number): string {
    return `${(i % 8) * 0.06}s`;
  }

  deleteProduct(id: number): void {
    if (!confirm('Delete this product?')) return;
    this.productService.delete(id).subscribe(() => {
      this.products = this.products.filter(p => p.productId !== id);
      this.filtered = this.filtered.filter(p => p.productId !== id);
    });
  }

  skeletons = [1,2,3,4,5,6,7,8];
}