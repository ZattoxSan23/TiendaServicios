using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; } = 0;

        // ✅ NUEVO: CategoryId como FK
        public int? CategoryId { get; set; }

        // ✅ Nombre de categoría (para compatibilidad)
        public string CategoryName { get; set; } = "General";

        public string Brand { get; set; } = "Generic";
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; } = false;
        public string? Sku { get; set; }
        public string? Tags { get; set; }
    }
}