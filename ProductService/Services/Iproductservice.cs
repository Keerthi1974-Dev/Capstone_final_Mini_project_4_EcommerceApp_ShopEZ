using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(ProductDTO dto);
        Task<bool> UpdateAsync(int id, ProductDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}