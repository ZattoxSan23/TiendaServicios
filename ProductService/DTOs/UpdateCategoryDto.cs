using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    public class UpdateCategoryDto
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }
}
