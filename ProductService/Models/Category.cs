// Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ RELACIÓN CON PRODUCTOS - CASCADE DELETE
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}