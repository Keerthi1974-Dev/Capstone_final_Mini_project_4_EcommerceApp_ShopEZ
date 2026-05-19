using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    public class ProductDTO
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 1000000)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string Category { get; set; } = string.Empty;
    }
}