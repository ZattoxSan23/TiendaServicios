using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductService.Data;
using ProductService.Services;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy => policy
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró 'DefaultConnection'");

builder.Services.AddDbContext<ProductDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ================= JWT - VERSIÓN ESTABLE (7.2.0 / 8.0.8) =================
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret no encontrado");
var key = Encoding.UTF8.GetBytes(jwtSecret);
var issuer = builder.Configuration["Jwt:Issuer"] ?? "AuthApp";
var audience = builder.Configuration["Jwt:Audience"] ?? "AuthUsers";

Console.WriteLine($"🔧 ProductService JWT Config: Issuer={issuer} | Audience={audience}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var header = context.Request.Headers["Authorization"].FirstOrDefault() ?? "";
                Console.WriteLine($"🔍 [JWT] Raw Header: '{header}'");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ [JWT] Authentication Failed: {context.Exception.Message}");
                if (context.Exception.InnerException != null)
                    Console.WriteLine($"   Inner: {context.Exception.InnerException.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ [JWT] TOKEN VÁLIDO");
                Console.WriteLine($"   User: {context.Principal?.Identity?.Name} | Role: {context.Principal?.FindFirst(ClaimTypes.Role)?.Value}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowFrontend");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    db.Database.EnsureCreated();
}

Console.WriteLine($"🚀 ProductService iniciado en http://localhost:5174");
app.Run();