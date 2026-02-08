using System.ComponentModel.DataAnnotations;

namespace AuthService.DTOs
{
    public class CreateUserDto
    {
        public string Username { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "Cliente";

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        // Opcional: solo si el admin quiere especificar fecha de creación (raro)
        public DateTime? CreatedAt { get; set; }
    }
}