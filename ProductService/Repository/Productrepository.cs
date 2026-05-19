using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
            => await _context.Products.ToListAsync();

        public async Task<Product?> GetByIdAsync(int id)
            => await _context.Products.FindAsync(id);

        public async Task AddAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            var existing = await _context.Products.FindAsync(product.ProductId)
                ?? throw new Exception("Product not found");

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.ImageUrl = product.ImageUrl;
            existing.Category = product.Category;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            var existing = await _context.Products.FindAsync(product.ProductId)
                ?? throw new Exception("Product not found");

            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}