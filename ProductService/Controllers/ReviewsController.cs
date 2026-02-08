using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.DTOs;

namespace ProductService.Controllers
{
    [Route("api/products/{productId}/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ProductDbContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/products/{productId}/reviews
        [HttpGet]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for product: {ProductId}", productId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // POST: api/products/{productId}/reviews
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, [FromBody] CreateReviewDto dto)
        {
            try
            {
                // Verificar si el producto existe
                var product = await _context.Products.FindAsync(productId);
                if (product == null || !product.IsActive)
                {
                    return NotFound(new { error = "Producto no encontrado" });
                }

                // Verificar si el usuario ya ha hecho una reseña
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == dto.UserId);

                if (existingReview != null)
                {
                    return BadRequest(new { error = "Ya has hecho una reseña para este producto" });
                }

                var review = new Review
                {
                    ProductId = productId,
                    UserId = dto.UserId,
                    Username = dto.Username,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync(); // Guardar primero la reseña

                // Actualizar rating promedio del producto
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .ToListAsync();

                // CORRECCIÓN: Convertir explícitamente el double a float
                product.Rating = reviews.Count > 0 ? (float)reviews.Average(r => r.Rating) : 0.0f;
                product.ReviewCount = reviews.Count;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Review added for product {ProductId} by user {UserId}", productId, dto.UserId);
                return CreatedAtAction(nameof(GetProductReviews), new { productId }, review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding review for product: {ProductId}", productId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }

   
}