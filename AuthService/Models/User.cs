using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!;

        // ✅ NUEVOS CAMPOS
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Campos de perfil
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}