namespace AuthService.Models
{
    public class UserRegisterDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Role { get; set; }   // opcional, solo admin lo pondrá "Admin"
    }
}