using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var users = _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Role,
                    u.PhoneNumber,
                    u.Address,
                    u.BirthDate,
                    u.CreatedAt
                })
                .ToList();

            return Ok(users);
        }

        [HttpPut("profile")]
        [Authorize]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Token inválido" });
            }

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound(new { error = "Usuario no encontrado" });

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
            {
                if (_context.Users.Any(u => u.Username == dto.Username && u.Id != userId))
                    return BadRequest(new { error = "El nombre de usuario ya está en uso" });
                user.Username = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                if (_context.Users.Any(u => u.Email == dto.Email && u.Id != userId))
                    return BadRequest(new { error = "El email ya está registrado" });
                user.Email = dto.Email;
            }

            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.Address != null) user.Address = dto.Address;

            if (!string.IsNullOrEmpty(dto.BirthDate))
            {
                if (!DateTime.TryParseExact(dto.BirthDate, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    return BadRequest(new { error = "Formato de fecha inválido. Use yyyy-MM-dd" });
                }
                user.BirthDate = parsedDate.Date;
            }

            _context.SaveChanges();

            var newToken = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Perfil actualizado correctamente",
                token = newToken,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.Role,
                    user.PhoneNumber,
                    user.Address,
                    user.BirthDate,
                    user.CreatedAt
                }
            });
        }

        [HttpPut("profile/password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Token inválido" });
            }

            var user = _context.Users.Find(userId);
            if (user == null) return NotFound(new { error = "Usuario no encontrado" });

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { error = "La contraseña actual es incorrecta" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _context.SaveChanges();

            return Ok(new { message = "Contraseña actualizada correctamente" });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] CreateUserDto dto)
        {
            if (_context.Users.Any(u => u.Username == dto.Username))
                return BadRequest(new { error = "El nombre de usuario ya existe" });

            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest(new { error = "El email ya está registrado" });

            var user = new User
            {
                Username = dto.Username,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                BirthDate = dto.BirthDate?.Date,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Usuario creado con éxito", userId = user.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserAdminDto dto)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound(new { error = "Usuario no encontrado" });

            if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
            {
                if (_context.Users.Any(u => u.Username == dto.Username && u.Id != id))
                    return BadRequest(new { error = "El nombre de usuario ya está en uso" });
                user.Username = dto.Username;
            }

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                if (_context.Users.Any(u => u.Email == dto.Email && u.Id != id))
                    return BadRequest(new { error = "El email ya está registrado" });
                user.Email = dto.Email;
            }

            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.Role != null) user.Role = dto.Role;
            if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
            if (dto.Address != null) user.Address = dto.Address;

            if (!string.IsNullOrEmpty(dto.BirthDate))
            {
                if (DateTime.TryParseExact(dto.BirthDate, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    user.BirthDate = parsedDate.Date;
                }
            }

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            _context.SaveChanges();

            return Ok(new { message = "Usuario actualizado correctamente" });
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeRole(int id, [FromBody] ChangeRoleDto dto)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            user.Role = dto.Role;
            _context.SaveChanges();

            return Ok(new { message = "Rol actualizado" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok(new { message = "Usuario eliminado" });
        }

        // ✅ MÉTODO PRIVADO - Dentro de la clase
        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("Jwt:Secret no encontrado");
            var issuer = _configuration["Jwt:Issuer"] ?? "PolleriaApp";
            var audience = _configuration["Jwt:Audience"] ?? "PolleriaUsers";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtSecret);

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
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}