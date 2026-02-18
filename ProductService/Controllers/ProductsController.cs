using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/products/featured
        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int count = 8)
        {
            try
            {
                var products = await _productService.GetFeaturedProductsAsync(count);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/products/filter
        [HttpGet("filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] ProductFilterDto filter)
        {
            try
            {
                var (products, totalCount, totalPages) = await _productService.GetProductsFilteredAsync(filter);

                return Ok(new
                {
                    products,
                    pagination = new
                    {
                        totalCount,
                        totalPages,
                        currentPage = filter.Page,
                        pageSize = filter.PageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new { error = "Producto no encontrado" });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product by id: {ProductId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/products/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _productService.GetCategoryListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/products/brands
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            try
            {
                var brands = await _productService.GetBrandsAsync();
                return Ok(brands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting brands");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // ✅ MANTENER: Endpoint original con string (NO MODIFICAR)
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(category);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category: {Category}", category);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // ✅ NUEVO: Endpoint con ID (ruta diferente)
        [HttpGet("category/id/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category id: {CategoryId}", categoryId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, dto);
                if (product == null)
                {
                    return NotFound(new { error = "Producto no encontrado" });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // PATCH: api/products/{id}/stock
        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            try
            {
                var success = await _productService.UpdateStockAsync(id, dto.Quantity);
                if (!success)
                {
                    return NotFound(new { error = "Producto no encontrado" });
                }
                return Ok(new { message = "Stock actualizado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product: {ProductId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _productService.DeleteProductAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Producto no encontrado" });
                }
                return Ok(new { message = "Producto eliminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }

    public class UpdateStockDto
    {
        public int Quantity { get; set; }
    }
}