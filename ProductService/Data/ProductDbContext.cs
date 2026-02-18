// Data/ProductDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.CategoryName); // Índice en el nombre para búsquedas
                entity.HasIndex(p => p.Brand);
                entity.HasIndex(p => p.Price);
                entity.HasIndex(p => p.IsFeatured);
                entity.HasIndex(p => p.IsActive);

                entity.Property(p => p.Price)
                    .HasPrecision(18, 2);

                entity.Property(p => p.DiscountPrice)
                    .HasPrecision(18, 2);

                // ✅ RELACIÓN CON CATEGORY - CASCADE DELETE
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade); // ✅ ELIMINA PRODUCTOS AL ELIMINAR CATEGORÍA
            });

            // Configuración de Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => r.ProductId);
                entity.HasIndex(r => r.UserId);
                entity.HasIndex(r => r.Rating);

                entity.HasOne(r => r.Product)
                    .WithMany()
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
                entity.HasIndex(c => c.IsActive);

                // ✅ RELACIÓN CON PRODUCTS - CASCADE DELETE CONFIGURADO ARRIBA
                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}