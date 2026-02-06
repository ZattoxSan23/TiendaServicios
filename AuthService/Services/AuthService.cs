using AuthService.Data;
using AuthService.Models;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    public class AuthServiceClass
    {
        private readonly AuthDbContext _context;
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        // Constructor: inyectamos IConfiguration para leer appsettings
        public AuthServiceClass(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;

            _jwtSecret = configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret no encontrado en appsettings.json");

            _issuer = configuration["Jwt:Issuer"]
                ?? "PolleriaApp";  // valor por defecto si no existe

            _audience = configuration["Jwt:Audience"]
                ?? "PolleriaUsers";
        }

        public string Register(UserRegisterDto dto)   // ← nuevo DTO
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = dto.Role ?? "Cliente",   // por defecto Cliente
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return "Usuario Registrado con Éxito";
        }

        public string? Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);  // Mejor UTF8 que ASCII

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}