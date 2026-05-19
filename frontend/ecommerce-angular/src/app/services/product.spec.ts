import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product';
import { Product } from '../models/product';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  const mockProduct: Product = {
    productId: 1,
    name: 'Test Product',
    description: 'Test Description',
    price: 500,
    stock: 10,
    category: 'Test',
    imageUrl: 'http://test.com/image.jpg'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ProductService]
    });
    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get all products', () => {
    service.getAll().subscribe(products => {
      expect(products.length).toBe(1);
      expect(products[0].name).toBe('Test Product');
    });

    const req = httpMock.expectOne('/api/products');
    expect(req.request.method).toBe('GET');
    req.flush([mockProduct]);
  });

  it('should get product by id', () => {
    service.getById(1).subscribe(product => {
      expect(product.productId).toBe(1);
      expect(product.price).toBe(500);
    });

    const req = httpMock.expectOne('/api/products/1');
    expect(req.request.method).toBe('GET');
    req.flush(mockProduct);
  });

  it('should create product with JSON', () => {
    const dto = {
      name: 'New Product',
      description: 'Desc',
      price: 1000,
      stock: 5,
      category: 'Test',
      imageUrl: 'http://test.com/img.jpg'
    };

    service.create(dto).subscribe(res => {
      expect(res).toBe('Product created successfully');
    });

    const req = httpMock.expectOne('/api/products');
    expect(req.request.method).toBe('POST');
    req.flush('Product created successfully');
  });

  it('should create product with file upload', () => {
    const formData = new FormData();
    formData.append('name', 'Test');

    service.createWithFile(formData).subscribe(res => {
      expect(res).toBe('Product created successfully');
    });

    const req = httpMock.expectOne('/api/products/upload');
    expect(req.request.method).toBe('POST');
    req.flush('Product created successfully');
  });

  it('should update product with JSON', () => {
    const dto = {
      name: 'Updated Product',
      description: 'Updated Desc',
      price: 600,
      stock: 8,
      category: 'Test'
    };

    service.update(1, dto).subscribe(res => {
      expect(res).toBe('Product updated successfully');
    });

    const req = httpMock.expectOne('/api/products/1');
    expect(req.request.method).toBe('PUT');
    req.flush('Product updated successfully');
  });

  it('should update product with file', () => {
    const formData = new FormData();
    formData.append('name', 'Updated');

    service.updateWithFile(1, formData).subscribe(res => {
      expect(res).toBe('Product updated successfully');
    });

    const req = httpMock.expectOne('/api/products/upload/1');
    expect(req.request.method).toBe('PUT');
    req.flush('Product updated successfully');
  });

  it('should delete product by id', () => {
    service.delete(1).subscribe(res => {
      expect(res).toBe('Product deleted successfully');
    });

    const req = httpMock.expectOne('/api/products/1');
    expect(req.request.method).toBe('DELETE');
    req.flush('Product deleted successfully');
  });
});