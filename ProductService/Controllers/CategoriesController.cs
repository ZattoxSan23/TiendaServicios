using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { error = "Categoría no encontrada" });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by id: {CategoryId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            // ✅ DEBUG: Verificar usuario autenticado
            Console.WriteLine($"📝 Create Category - User: {User?.Identity?.Name}");
            Console.WriteLine($"📝 IsAuthenticated: {User?.Identity?.IsAuthenticated}");
            Console.WriteLine($"📝 Claims count: {User?.Claims?.Count() ?? 0}");

            foreach (var claim in User?.Claims ?? Enumerable.Empty<System.Security.Claims.Claim>())
            {
                Console.WriteLine($"   Claim: {claim.Type} = {claim.Value}");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, dto);
                if (category == null)
                {
                    return NotFound(new { error = "Categoría no encontrada" });
                }
                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPatch("{id}/toggle")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _categoryService.ToggleCategoryStatusAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Categoría no encontrada" });
                }
                return Ok(new { message = "Estado actualizado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _categoryService.DeleteCategoryAsync(id);
                if (!success)
                {
                    return NotFound(new { error = "Categoría no encontrada" });
                }
                return Ok(new { message = "Categoría y productos asociados eliminados" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}