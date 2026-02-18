using ProductService.DTOs;

namespace ProductService.Services
{
    public interface IProductService
    {
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync(int count = 8);

        // ✅ MANTENER EXACTAMENTE IGUAL (no tocar)
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);

        // ✅ NUEVO: Método para filtrar por ID (nombre diferente)
        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId);

        Task<(IEnumerable<ProductDto> Products, int TotalCount, int TotalPages)> GetProductsFilteredAsync(ProductFilterDto filter);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int id, int quantity);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<CategoryDto>> GetCategoryListAsync();
        Task<IEnumerable<string>> GetBrandsAsync();
    }
}