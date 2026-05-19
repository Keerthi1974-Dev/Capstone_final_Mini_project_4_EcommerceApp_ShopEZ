import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product';
import { Product } from '../../../models/product';

@Component({
  selector: 'app-admin-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-products.html',
  styleUrl: './admin-products.css'
})
export class AdminProducts implements OnInit {

  products: Product[] = [];
  loading = false;
  message = '';
  error = '';
  showForm = false;

  // Form fields
  name = '';
  description = '';
  price = 0;
  stock = 0;
  category = 'Women';
  imageUrl = '';

  // Cloudinary file upload
  selectedFile: File | null = null;
  previewUrl: string | null = null;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.productService.getAll().subscribe({
      next: (data) => {
        this.products = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load products';
        this.loading = false;
      }
    });
  }

  toggleForm(): void {
    this.showForm = !this.showForm;
    this.clearForm();
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(this.selectedFile);
      this.imageUrl = ''; // clear URL field if file picked
    }
  }

  addProduct(): void {
    if (!this.name || !this.price || !this.stock) {
      this.error = 'Please fill all required fields';
      return;
    }
    if (!this.selectedFile && !this.imageUrl) {
      this.error = 'Please upload an image or provide image URL';
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';

    // if file selected → FormData to /upload, else JSON to /api/products
    if (this.selectedFile) {
      const formData = new FormData();
      formData.append('name', this.name);
      formData.append('description', this.description);
      formData.append('price', this.price.toString());
      formData.append('stock', this.stock.toString());
      formData.append('category', this.category);
      formData.append('imageFile', this.selectedFile);

      this.productService.createWithFile(formData).subscribe({
        next: () => this.onSuccess(),
        error: (err) => this.onError(err)
      });
    } else {
      //if  No file selected — send plain JSON body
      const dto = {
        name: this.name,
        description: this.description,
        price: this.price,
        stock: this.stock,
        category: this.category,
        imageUrl: this.imageUrl
      };

      this.productService.create(dto).subscribe({
        next: () => this.onSuccess(),
        error: (err) => this.onError(err)
      });
    }
  }

  private onSuccess(): void {
    this.message = 'Product added successfully!';
    this.loading = false;
    this.clearForm();
    this.showForm = false;
    this.loadProducts();
    setTimeout(() => this.message = '', 3000);
  }

  private onError(err: any): void {
    this.error = 'Failed to add product: ' + (err.error || err.message);
    this.loading = false;
    setTimeout(() => this.error = '', 5000);
  }

  deleteProduct(id: number): void {
    if (!confirm('Are you sure you want to delete this product?')) return;

    this.productService.delete(id).subscribe({
      next: () => {
        this.message = 'Product deleted!';
        this.loadProducts();
        setTimeout(() => this.message = '', 3000);
      },
      error: () => {
        this.error = 'Failed to delete product';
        setTimeout(() => this.error = '', 3000);
      }
    });
  }

  clearForm(): void {
    this.name = '';
    this.description = '';
    this.price = 0;
    this.stock = 0;
    this.category = 'Women';
    this.imageUrl = '';
    this.selectedFile = null;
    this.previewUrl = null;
    this.error = '';
  }
}