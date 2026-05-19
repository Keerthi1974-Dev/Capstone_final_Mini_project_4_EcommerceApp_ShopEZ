using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    public class OrderDTO
    {
        public int UserId { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();
    }

    public class OrderItemDTO
    {
        [Required] public int ProductId { get; set; }
        [Range(1, 100)] public int Quantity { get; set; }
        [Range(0, 1000000)] public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemResponseDTO> Items { get; set; } = new();
    }

    public class OrderItemResponseDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    // DTO used when calling Product Service internally
    public class ProductInfoDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}