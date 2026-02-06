using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthServiceClass _authService;

        public AuthController(AuthServiceClass authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public IActionResult Register(UserRegisterDto dto)
        {
            return Ok(_authService.Register(dto));
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var token = _authService.Login(request);
            if (token == null) return Unauthorized();
            return Ok(new { Token = token });

        }
    }

}