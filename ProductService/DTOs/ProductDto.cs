namespace ProductService.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }

        // ✅ NUEVOS CAMPOS DE CATEGORÍA
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = "General";
        public string? CategoryImageUrl { get; set; }

        public string Brand { get; set; } = "Generic";
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public float Rating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Sku { get; set; }
        public string? Tags { get; set; }

        // Propiedades calculadas
        public decimal FinalPrice => DiscountPrice ?? Price;
        public decimal DiscountPercentage => DiscountPrice.HasValue
            ? Math.Round((1 - DiscountPrice.Value / Price) * 100, 0)
            : 0;
        public bool HasDiscount => DiscountPrice.HasValue;
    }
}