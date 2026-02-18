namespace ProductService.DTOs
{
    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? Stock { get; set; }

        // ✅ NUEVO: Puede cambiar de categoría por ID
        public int? CategoryId { get; set; }

        // ✅ O por nombre
        public string? CategoryName { get; set; }

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
}