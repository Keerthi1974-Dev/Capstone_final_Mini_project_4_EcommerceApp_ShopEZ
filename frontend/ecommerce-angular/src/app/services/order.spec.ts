import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController
} from '@angular/common/http/testing';

import { OrderService } from './order';
import { Order, OrderDTO } from '../models/order';
import { environment } from '../../environments/environment';

describe('OrderService', () => {

  let service: OrderService;
  let httpMock: HttpTestingController;

 
  const baseUrl = `${environment.apiUrl}/api/orders`;

  const mockOrder: Order = {
    orderId: 1,
    userId: 7,
    orderDate: '2026-05-17T10:00:00Z',
    totalAmount: 1000,
    items: [
      {
        productId: 1,
        productName: 'Test Product',
        quantity: 2,
        price: 500,
        imageUrl: ''
      }
    ]
  };

  const mockOrder2: Order = {
    orderId: 2,
    userId: 7,
    orderDate: '2026-05-18T10:00:00Z',
    totalAmount: 300,
    items: [
      {
        productId: 2,
        productName: 'Another Product',
        quantity: 1,
        price: 300,
        imageUrl: ''
      }
    ]
  };

  const mockDto: OrderDTO = {
    items: [
      {
        productId: 1,
        quantity: 2,
        price: 500
      }
    ]
  };

  beforeEach(() => {

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [OrderService]
    });

    service  = TestBed.inject(OrderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  // ─── Creation ────────────────────────────────────────────────────────────────

  it('should create service', () => {

    expect(service).toBeTruthy();
  });

  // ─── getAll ──────────────────────────────────────────────────────────────────

  it('should get all orders', () => {

    service.getAll().subscribe((orders: Order[]) => {

      expect(orders.length).toBe(1);
      expect(orders[0].orderId).toBe(1);
    });

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([mockOrder]);
  });

  it('should get all orders and return multiple orders', () => {

    service.getAll().subscribe((orders: Order[]) => {

      expect(orders.length).toBe(2);
      expect(orders[0].orderId).toBe(1);
      expect(orders[1].orderId).toBe(2);
    });

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([mockOrder, mockOrder2]);
  });

  it('should return empty array when no orders exist', () => {

    service.getAll().subscribe((orders: Order[]) => {

      expect(orders.length).toBe(0);
    });

    const req = httpMock.expectOne(baseUrl);
    req.flush([]);
  });

  // ─── getById ─────────────────────────────────────────────────────────────────

  it('should get order by id', () => {

    service.getById(1).subscribe((order: Order) => {

      expect(order.orderId).toBe(1);
      expect(order.totalAmount).toBe(1000);
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockOrder);
  });

  it('should get correct order details by id', () => {

    service.getById(1).subscribe((order: Order) => {

      expect(order.userId).toBe(7);
      expect(order.items.length).toBe(1);
      expect(order.items[0].productName).toBe('Test Product');
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);
    req.flush(mockOrder);
  });

  it('should handle error when order not found', () => {

    service.getById(999).subscribe({
      next: () => fail('expected an error'),
      error: (err) => {
        expect(err.status).toBe(404);
      }
    });

    const req = httpMock.expectOne(`${baseUrl}/999`);
    req.flush('Not found', { status: 404, statusText: 'Not Found' });
  });

  // ─── createOrder ─────────────────────────────────────────────────────────────

  it('should create order', () => {

    service.createOrder(mockDto).subscribe((order: Order) => {

      expect(order.orderId).toBe(1);
      expect(order.totalAmount).toBe(1000);
    });

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockDto);
    req.flush(mockOrder);
  });

  it('should send correct order payload on create', () => {

    service.createOrder(mockDto).subscribe();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.body.items.length).toBe(1);
    expect(req.request.body.items[0].productId).toBe(1);
    expect(req.request.body.items[0].quantity).toBe(2);
    expect(req.request.body.items[0].price).toBe(500);
    req.flush(mockOrder);
  });

  it('should handle error when create order fails', () => {

    service.createOrder(mockDto).subscribe({
      next: () => fail('expected an error'),
      error: (err) => {
        expect(err.status).toBe(400);
      }
    });

    const req = httpMock.expectOne(baseUrl);
    req.flush('Bad request', { status: 400, statusText: 'Bad Request' });
  });

  // ─── deleteOrder ─────────────────────────────────────────────────────────────

  it('should delete order', () => {

    service.deleteOrder(1).subscribe((response: string) => {

      expect(response).toBe('Order deleted successfully!!');
    });

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush('Order deleted successfully!!');
  });

  it('should send DELETE request to correct url', () => {

    service.deleteOrder(2).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/2`);
    expect(req.request.method).toBe('DELETE');
    req.flush('Order deleted successfully!!');
  });

  it('should handle error when delete order fails', () => {

    service.deleteOrder(999).subscribe({
      next: () => fail('expected an error'),
      error: (err) => {
        expect(err.status).toBe(404);
      }
    });

    const req = httpMock.expectOne(`${baseUrl}/999`);
    req.flush('Not found', { status: 404, statusText: 'Not Found' });
  });

});