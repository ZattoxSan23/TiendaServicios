namespace AuthService.DTOs
{
    public class UserRegisterDto
    {
        public string Username { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;


        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Role { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? BirthDate { get; set; }
    }
}