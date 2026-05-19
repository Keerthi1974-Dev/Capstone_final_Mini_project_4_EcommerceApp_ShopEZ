import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product } from '../models/product';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private baseUrl = `${environment.apiUrl}/api/products`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  getById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  //  when a file is selected → POST to /api/products/upload (multipart/form-data)
  createWithFile(formData: FormData): Observable<string> {
    return this.http.post(`${this.baseUrl}/upload`, formData, {
      responseType: 'text'
    });
  }

  // when only a URL is provided → POST to /api/products with JSON body
  create(dto: {
    name: string;
    description: string;
    price: number;
    stock: number;
    category: string;
    imageUrl?: string;
  }): Observable<string> {
    return this.http.post(`${this.baseUrl}`, dto, {
      responseType: 'text'
    });
  }

  // PUT with image file → /api/products/upload/{id}
  updateWithFile(id: number, formData: FormData): Observable<string> {
    return this.http.put(`${this.baseUrl}/upload/${id}`, formData, {
      responseType: 'text'
    });
  }

  // PUT with JSON (no file)
  update(id: number, dto: {
    name: string;
    description: string;
    price: number;
    stock: number;
    category: string;
    imageUrl?: string;
  }): Observable<string> {
    return this.http.put(`${this.baseUrl}/${id}`, dto, {
      responseType: 'text'
    });
  }

  delete(id: number): Observable<string> {
    return this.http.delete(`${this.baseUrl}/${id}`, {
      responseType: 'text'
    });
  }
}