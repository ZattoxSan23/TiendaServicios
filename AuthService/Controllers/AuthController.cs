using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
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
            try
            {
                var result = _authService.Register(dto);
                return Ok(new { message = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { error = "Usuario y contraseña son requeridos" });
                }

                var token = _authService.Login(request);

                if (token == null)
                {
                    return Unauthorized(new { error = "Credenciales inválidas" });
                }

                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login: {ex}");
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }
    }
}