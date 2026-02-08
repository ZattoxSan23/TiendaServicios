namespace AuthService.DTOs
{
    public class UpdateUserAdminDto
    {
        public string? Username { get; set; }
        public string? FirstName { get; set; }     // ✅ NUEVO
        public string? LastName { get; set; }      // ✅ NUEVO
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? BirthDate { get; set; }
        public string? Password { get; set; }
    }
}
