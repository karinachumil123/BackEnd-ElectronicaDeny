using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BackEnd_ElectronicaDeny.DTOs;
using BackEnd_ElectronicaDeny.Services;


namespace BackEnd_ElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;


        public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController> logger, IAuthService authService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                _logger.LogInformation("Intento de inicio de sesión para el usuario: {Email}", login.Email);

                // Primero, obtener el usuario y sus relaciones
                var usuario = await _context.Usuarios
                    .Include(u => u.Estado)
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Correo == login.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", login.Email);
                    return Unauthorized("Credenciales incorrectas");
                }

                // Verificar si el usuario está activo

                var estado = await _context.Estados.FindAsync(usuario.EstadoId);
                if (estado == null || estado.Nombre != "Activo")
                {
                    _logger.LogWarning("Usuario inactivo: {Email}", login.Email);
                    return Unauthorized("Usuario inactivo");
                }

                // Verificar la contraseña
                if (usuario.Contrasena != login.Password)
                {
                    _logger.LogWarning("Contraseña incorrecta para el usuario: {Email}", login.Email);
                    return Unauthorized("Credenciales incorrectas");
                }

                // Obtener el rol
                var rol = await _context.Roles.FindAsync(usuario.RolId);
                if (rol == null)
                {
                    _logger.LogError("Rol no encontrado para el usuario: {Email}", login.Email);
                    return StatusCode(500, "Error en la configuración del usuario");
                }
               

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Role, rol.Nombre),
                    new Claim("Email", usuario.Correo),
                };

               

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

                // Actualizar último inicio de sesión
                usuario.UltimoInicioSesion = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Inicio de sesión exitoso para el usuario: {Email}", login.Email);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    userId = usuario.Id,
                    userName = usuario.Nombre,
                    userApellido = usuario.Apellido,
                    userEmail = usuario.Correo,
                    userRole = rol.Nombre,
                   
                    userImage = usuario.Imagen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el inicio de sesión para {Email}", login.Email);
                return StatusCode(500, "Error interno del servidor");
            }

        }
        [HttpPost("recuperar-contrasena")]
        public async Task<IActionResult> RecuperarContrasena([FromBody] string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                return BadRequest(new { mensaje = "El correo es obligatorio." });
            }

            var resultado = await _authService.EnviarCodigoRecuperacionAsync(correo);
            if (!resultado)
            {
                return NotFound(new { mensaje = "Correo no encontrado o error al enviar el código" });
            }

            return Ok(new { mensaje = "Código de recuperación enviado exitosamente" });
        }

       


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var result = await _authService.ResetPasswordAsync(model);
            if (!result)
                return BadRequest(new { message = "Código inválido o correo incorrecto." });

            return Ok(new { message = "Contraseña actualizada correctamente." });
        }

    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}