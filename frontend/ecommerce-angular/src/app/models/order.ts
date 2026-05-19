export interface OrderItemDTO {
  productId: number;
  quantity: number;
  price: number;
}

export interface OrderDTO {
  items: OrderItemDTO[];
}

export interface OrderItemResponse {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
  imageUrl?: string;
}

export interface Order {
  orderId: number;
  userId: number;
  orderDate: string;
  totalAmount: number;
  items: OrderItemResponse[]; 
}