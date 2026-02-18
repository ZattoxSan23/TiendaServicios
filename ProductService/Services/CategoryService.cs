using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ProductDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    ProductCount = c.Products.Count(p => p.IsActive)
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                ProductCount = category.Products.Count(p => p.IsActive)
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (existing != null)
            {
                throw new InvalidOperationException($"La categoría '{dto.Name}' ya existe");
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                ProductCount = 0
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != category.Name)
            {
                var existing = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

                if (existing != null)
                {
                    throw new InvalidOperationException($"La categoría '{dto.Name}' ya existe");
                }
            }

            if (!string.IsNullOrEmpty(dto.Name)) category.Name = dto.Name;
            if (dto.Description != null) category.Description = dto.Description;
            if (dto.ImageUrl != null) category.ImageUrl = dto.ImageUrl;
            if (dto.IsActive.HasValue) category.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Category updated: {CategoryId}", category.Id);

            return await GetCategoryByIdAsync(id);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return false;

            // Eliminar reviews de los productos primero
            foreach (var product in category.Products.ToList())
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == product.Id)
                    .ToListAsync();

                _context.Reviews.RemoveRange(reviews);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Category deleted: {CategoryId} - {ProductCount} products removed",
                id, category.Products.Count);

            return true;
        }

        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.IsActive = !category.IsActive;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Category status toggled: {CategoryId} - IsActive: {IsActive}",
                id, category.IsActive);

            return true;
        }
    }
}