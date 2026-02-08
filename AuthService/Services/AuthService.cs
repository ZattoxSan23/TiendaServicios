using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
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

        public AuthServiceClass(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;

            _jwtSecret = configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret no encontrado en appsettings.json");

            _issuer = configuration["Jwt:Issuer"] ?? "PolleriaApp";
            _audience = configuration["Jwt:Audience"] ?? "PolleriaUsers";
        }

        public string Register(UserRegisterDto dto)
        {
            // Validar y convertir BirthDate
            DateTime? birthDateParsed = null;
            if (!string.IsNullOrEmpty(dto.BirthDate))
            {
                if (!DateTime.TryParseExact(dto.BirthDate, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    throw new ArgumentException("Formato de fecha inválido. Use yyyy-MM-dd");
                }
                birthDateParsed = parsedDate.Date;
            }

            // Verificar duplicados
            if (_context.Users.Any(u => u.Username == dto.Username))
                throw new InvalidOperationException("El nombre de usuario ya existe");

            if (_context.Users.Any(u => u.Email == dto.Email))
                throw new InvalidOperationException("El email ya está registrado");

            var user = new User
            {
                Username = dto.Username,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role ?? "Cliente",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),  // ✅ CORREGIDO
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                BirthDate = birthDateParsed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return "Usuario Registrado con Éxito";
        }

        public string? Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null)
            {
                Console.WriteLine($"Usuario no encontrado: {request.Username}");
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))  // ✅ CORREGIDO
            {
                Console.WriteLine($"Contraseña incorrecta para: {request.Username}");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("username", user.Username),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("lastName", user.LastName ?? ""),
                new Claim("email", user.Email),
                new Claim("phoneNumber", user.PhoneNumber ?? ""),
                new Claim("address", user.Address ?? ""),
                new Claim("birthDate", user.BirthDate?.ToString("yyyy-MM-dd") ?? ""),
                new Claim("createdAt", user.CreatedAt.ToString("o"))
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
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