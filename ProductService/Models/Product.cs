using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        public int Stock { get; set; } = 0;

        [MaxLength(50)]
        public string Category { get; set; } = "General";

        [MaxLength(50)]
        public string Brand { get; set; } = "Generic";

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? Size { get; set; }

        [MaxLength(50)]
        public string? Material { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        public float Rating { get; set; } = 0.0f;

        public int ReviewCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string? Sku { get; set; }

        [MaxLength(100)]
        public string? Tags { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public decimal FinalPrice => DiscountPrice ?? Price;

        [NotMapped]
        public decimal DiscountPercentage => DiscountPrice.HasValue
            ? Math.Round((1 - DiscountPrice.Value / Price) * 100, 0)
            : 0;

        [NotMapped]
        public bool HasDiscount => DiscountPrice.HasValue;
    }
}