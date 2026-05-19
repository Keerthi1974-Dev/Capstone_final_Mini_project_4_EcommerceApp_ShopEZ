using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services
{
    public class OrderServiceImpl : IOrderService
    {
        private readonly OrderDbContext _context;
        private readonly ProductServiceClient _productClient;

        public OrderServiceImpl(OrderDbContext context, ProductServiceClient productClient)
        {
            _context = context;
            _productClient = productClient;
        }

        public async Task<OrderResponseDTO> CreateOrderAsync(OrderDTO dto, string? bearerToken = null)
        {
            if (dto == null || dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Order items cannot be empty");

            decimal total = 0;
            var orderItems = new List<OrderItem>();
            var responseItems = new List<OrderItemResponseDTO>();

            foreach (var item in dto.Items)
            {
                if (item.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than zero");

                // Call Product Service to get product info
                var product = await _productClient.GetProductAsync(item.ProductId, bearerToken);
                if (product == null)
                    throw new ArgumentException($"Product {item.ProductId} not found");

                var lineTotal = product.Price * item.Quantity;
                total += lineTotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    Quantity = item.Quantity,
                    Price = product.Price
                });

                responseItems.Add(new OrderItemResponseDTO
                {
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl
                });
            }

            var order = new Order
            {
                UserId = dto.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = total,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return new OrderResponseDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = responseItems
            };
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();

            return orders.Select(order => new OrderResponseDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(i => new OrderItemResponseDTO
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    ImageUrl = i.ProductImageUrl
                }).ToList()
            });
        }

        public async Task<OrderResponseDTO?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return null;

            return new OrderResponseDTO
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(i => new OrderItemResponseDTO
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    ImageUrl = i.ProductImageUrl
                }).ToList()
            };
        }

        public async Task<bool> UpdateAsync(int id, OrderDTO dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return false;

            decimal total = 0;
            var newItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                total += item.Price * item.Quantity;
                newItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            order.OrderItems = newItems;
            order.TotalAmount = total;
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}