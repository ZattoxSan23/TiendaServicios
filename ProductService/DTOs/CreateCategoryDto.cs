using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }
    }
}
