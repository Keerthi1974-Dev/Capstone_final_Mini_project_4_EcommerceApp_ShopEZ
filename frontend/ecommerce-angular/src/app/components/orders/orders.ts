import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../services/order';
import { Order } from '../../models/order';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  error = false;
  expandedOrderId: number | null = null;

  // added ChangeDetectorRef here
  constructor(
    private orderService: OrderService,
    private cd: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.orderService.getAll().subscribe({
      next: (data) => {
        this.orders = [...data].reverse();
        this.loading = false;
        this.cd.detectChanges(); // force UI update
      },
      error: () => {
        this.loading = false;
        this.error = true;
        this.cd.detectChanges(); // same here
      }
    });
  }

  toggleOrder(orderId: number): void {
    this.expandedOrderId = this.expandedOrderId === orderId ? null : orderId;
  }

  getTotalItems(order: Order): number {
    return order.items?.reduce((sum, item) => sum + item.quantity, 0) ?? 0;
  }

  skeletons = [1, 2, 3];
}