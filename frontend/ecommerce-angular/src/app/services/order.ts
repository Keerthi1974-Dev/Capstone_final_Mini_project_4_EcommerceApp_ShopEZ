import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Order, OrderDTO } from '../models/order';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private baseUrl = `${environment.apiUrl}/api/orders`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Order[]> {
    return this.http.get<Order[]>(this.baseUrl);
  }

  getById(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  createOrder(dto: OrderDTO): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, dto);
  }

  deleteOrder(id: number): Observable<string> {
    return this.http.delete(`${this.baseUrl}/${id}`, { responseType: 'text' });
  }
}