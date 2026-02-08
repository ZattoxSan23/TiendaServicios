using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<(IEnumerable<ProductDto> Products, int TotalCount, int TotalPages)> GetProductsFilteredAsync(ProductFilterDto filter);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<string>> GetBrandsAsync();
    }

    public class ProductService : IProductService
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return null;

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8)
        {
            var products = await _context.Products
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _context.Products
                .Where(p => p.IsActive && p.Category == category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalCount, int TotalPages)> GetProductsFilteredAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(p =>
                    p.Name.Contains(filter.Search) ||
                    p.Description.Contains(filter.Search) ||
                    p.Tags != null && p.Tags.Contains(filter.Search));
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(p => p.Category == filter.Category);
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                query = query.Where(p => p.Brand == filter.Brand);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            if (filter.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
            }

            if (filter.OnDiscount.HasValue && filter.OnDiscount.Value)
            {
                query = query.Where(p => p.DiscountPrice.HasValue);
            }

            if (filter.InStock.HasValue && filter.InStock.Value)
            {
                query = query.Where(p => p.Stock > 0);
            }

            // Ordenar
            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.Rating),
                _ => query.OrderByDescending(p => p.CreatedAt) // newest por defecto
            };

            // Paginación
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (products.Select(MapToDto), totalCount, totalPages);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                Stock = dto.Stock,
                Category = dto.Category,
                Brand = dto.Brand,
                Color = dto.Color,
                Size = dto.Size,
                Material = dto.Material,
                ImageUrl = dto.ImageUrl,
                IsFeatured = dto.IsFeatured,
                Sku = dto.Sku,
                Tags = dto.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
            return MapToDto(product);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            // Actualizar solo los campos proporcionados
            if (!string.IsNullOrEmpty(dto.Name)) product.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) product.Description = dto.Description;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            product.DiscountPrice = dto.DiscountPrice;
            if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
            if (!string.IsNullOrEmpty(dto.Category)) product.Category = dto.Category;
            if (!string.IsNullOrEmpty(dto.Brand)) product.Brand = dto.Brand;
            product.Color = dto.Color;
            product.Size = dto.Size;
            product.Material = dto.Material;
            product.ImageUrl = dto.ImageUrl;
            if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;
            if (dto.IsFeatured.HasValue) product.IsFeatured = dto.IsFeatured.Value;
            product.Sku = dto.Sku;
            product.Tags = dto.Tags;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated: {ProductId}", product.Id);
            return MapToDto(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            // Soft delete (cambiar estado)
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Product soft-deleted: {ProductId}", product.Id);
            return true;
        }

        public async Task<bool> UpdateStockAsync(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.Stock = quantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            return await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                Category = product.Category,
                Brand = product.Brand,
                Color = product.Color,
                Size = product.Size,
                Material = product.Material,
                ImageUrl = product.ImageUrl,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                IsActive = product.IsActive,
                IsFeatured = product.IsFeatured,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Sku = product.Sku,
                Tags = product.Tags
            };
        }
    }
}