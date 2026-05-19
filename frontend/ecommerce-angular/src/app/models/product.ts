export interface Product {
  productId: number;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  stock: number;
  category: string;
}

export interface ProductDTO {
  name: string;
  description: string;
  price: number;
  imageUrl?: string;    // set by Cloudinary
  stock: number;
  category: string;
}