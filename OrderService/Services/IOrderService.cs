using OrderService.DTOs;

namespace OrderService.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(OrderDTO dto, string? bearerToken = null);
        Task<IEnumerable<OrderResponseDTO>> GetAllAsync();
        Task<OrderResponseDTO?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, OrderDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}