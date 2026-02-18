namespace ProductService.DTOs
{
    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }      // Por nombre (compatibilidad)
        public int? CategoryId { get; set; }       // ✅ NUEVO: Por ID
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