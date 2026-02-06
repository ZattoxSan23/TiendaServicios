using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }
       
        public string Username { get; set; } = null!;
       
        public string PasswordHash { get; set; } = null!;
        public String Role { get; set; }

        public string Email { get; set; } = null!;
       
    }
}
