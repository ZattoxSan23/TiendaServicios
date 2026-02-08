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
        public string Category { get; set; } = "General";
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

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; } = 0;
        public string Category { get; set; } = "General";
        public string Brand { get; set; } = "Generic";
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; } = false;
        public string? Sku { get; set; }
        public string? Tags { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? Stock { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public string? Sku { get; set; }
        public string? Tags { get; set; }
    }

    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? OnDiscount { get; set; }
        public bool? InStock { get; set; }
        public string? SortBy { get; set; } // "price_asc", "price_desc", "newest", "rating"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}