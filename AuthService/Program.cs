using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ✅ LOGGING DETALLADO
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("No se encontró 'DefaultConnection'");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// Servicio de autenticación
builder.Services.AddScoped<AuthServiceClass>();

// ✅ CONFIGURACIÓN JWT - USAR EXACTAMENTE LOS MISMOS VALORES QUE EL SERVICIO
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret no encontrado");
var key = Encoding.UTF8.GetBytes(jwtSecret);

// ✅ LEER DE APPSettings O USAR LOS MISMOS DEFAULTS HARDCODEADOS
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuthApp";  // ✅ MISMO DEFAULT
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AuthUsers"; // ✅ MISMO DEFAULT

Console.WriteLine($"🔧 AuthService JWT Config:");
Console.WriteLine($"🔧   Secret length: {jwtSecret.Length}");
Console.WriteLine($"🔧   Issuer: {jwtIssuer}");
Console.WriteLine($"🔧   Audience: {jwtAudience}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,  // ✅ USAR VARIABLE
            ValidateAudience = true,
            ValidAudience = jwtAudience, // ✅ USAR VARIABLE
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline - ORDEN CRÍTICO
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ NO USAR HTTPS REDIRECTION EN DESARROLLO - CAUSA PROBLEMAS CON CORS
// app.UseHttpsRedirection(); // ❌ COMENTADO EN DESARROLLO

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Verificar configuración al iniciar
Console.WriteLine($"🚀 AuthService iniciado en http://localhost:5003");
Console.WriteLine($"🚀 JWT Issuer configurado: {jwtIssuer}");
Console.WriteLine($"🚀 JWT Audience configurado: {jwtAudience}");

app.Run();