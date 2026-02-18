using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
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
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return null;

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        // ✅ MANTENER: Método original con string (NO MODIFICAR)
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryName == category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        // ✅ NUEVO: Método con int (nombre diferente)
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return products.Select(MapToDto);
        }

        public async Task<(IEnumerable<ProductDto> Products, int TotalCount, int TotalPages)> GetProductsFilteredAsync(ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filtro de búsqueda
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(p =>
                    p.Name.Contains(filter.Search) ||
                    p.Description.Contains(filter.Search) ||
                    p.Tags != null && p.Tags.Contains(filter.Search));
            }

            // ✅ Filtro por CategoryId (int) - prioridad alta
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }
            // Filtro por Category (string) - compatibilidad
            else if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(p => p.CategoryName == filter.Category);
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
                _ => query.OrderByDescending(p => p.CreatedAt)
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
            // Verificar que la categoría existe si se proporciona CategoryId
            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                {
                    throw new InvalidOperationException($"La categoría con ID {dto.CategoryId} no existe");
                }
                // Sincronizar el nombre
                dto.CategoryName = category.Name;
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId,
                CategoryName = dto.CategoryName ?? "General",
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
            return await GetProductByIdAsync(product.Id) ?? throw new InvalidOperationException("Error retrieving created product");
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return null;

            // Si cambia la categoría por ID, actualizar ambos campos
            if (dto.CategoryId.HasValue && dto.CategoryId != product.CategoryId)
            {
                var category = await _context.Categories.FindAsync(dto.CategoryId.Value);
                if (category == null)
                {
                    throw new InvalidOperationException($"La categoría con ID {dto.CategoryId} no existe");
                }
                product.CategoryId = dto.CategoryId;
                product.CategoryName = category.Name;
            }
            // Si solo cambia el nombre
            else if (!string.IsNullOrEmpty(dto.CategoryName) && dto.CategoryName != product.CategoryName)
            {
                product.CategoryName = dto.CategoryName;
            }

            if (!string.IsNullOrEmpty(dto.Name)) product.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Description)) product.Description = dto.Description;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            product.DiscountPrice = dto.DiscountPrice;
            if (dto.Stock.HasValue) product.Stock = dto.Stock.Value;
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
            return await GetProductByIdAsync(product.Id);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

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
            return await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => c.Name)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoryListAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
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
                CategoryId = product.CategoryId,
                CategoryName = product.CategoryName,
                CategoryImageUrl = product.Category?.ImageUrl,
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