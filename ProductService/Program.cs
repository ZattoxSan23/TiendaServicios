using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.Data;
using ProductService.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Habilitar CORS para el frontend (Next.js en puerto 3000)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000")   // puerto de tu frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // si usas cookies en el futuro
    });
});

// ────────────────────────────────────────────────
// Servicios básicos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ────────────────────────────────────────────────
// Conexión a PostgreSQL desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró 'DefaultConnection' en appsettings.json");

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(connectionString));

// ────────────────────────────────────────────────
// Servicio de productos
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();

// ────────────────────────────────────────────────
// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret no encontrado en appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "PolleriaApp",

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PolleriaUsers",

            ClockSkew = TimeSpan.Zero             // Sin tolerancia de tiempo
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowNextJs");  // ← ponlo ANTES de UseAuthentication y UseAuthorization

// ────────────────────────────────────────────────
// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();      // Buena práctica
app.UseAuthentication();        // ¡IMPORTANTE! Antes de Authorization
app.UseAuthorization();

app.MapControllers();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

    // Asegurar que la base de datos está creada
    await dbContext.Database.EnsureCreatedAsync();

  
}

app.Run();