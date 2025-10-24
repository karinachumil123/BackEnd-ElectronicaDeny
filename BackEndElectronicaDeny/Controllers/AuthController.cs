using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BackEnd_ElectronicaDeny.DTOs;
using BackEnd_ElectronicaDeny.Services;
using BackEndElectronicaDeny.DTOs;

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
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AuthController(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IAuthService authService,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _authService = authService;
            _passwordHasher = passwordHasher;
        }

        // ===========================
        // LOGIN (verifica contra HASH)
        // ===========================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                _logger.LogInformation("Intento de inicio de sesión para el usuario: {Email}", login.Email);

                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Correo == login.Email);

                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {Email}", login.Email);
                    return Unauthorized("Credenciales incorrectas");
                }

                if (usuario.Estado != 1)
                {
                    _logger.LogWarning("Usuario inactivo: {Email}", login.Email);
                    return Unauthorized("Usuario inactivo");
                }

                // Verificar contraseña HASH
                var verification = _passwordHasher.VerifyHashedPassword(usuario, usuario.Contrasena, login.Password);
                if (verification == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Contraseña incorrecta para el usuario: {Email}", login.Email);
                    return Unauthorized("Credenciales incorrectas");
                }

                // (Opcional) Re-hash si el algoritmo cambió
                if (verification == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    usuario.Contrasena = _passwordHasher.HashPassword(usuario, login.Password);
                    await _context.SaveChangesAsync();
                }

                // Obtener rol y permisos
                var rol = usuario.Rol ?? await _context.Roles.FindAsync(usuario.RolId);
                if (rol == null)
                {
                    _logger.LogError("Rol no encontrado para el usuario: {Email}", login.Email);
                    return StatusCode(500, "Error en la configuración del usuario");
                }

                var permisos = await _context.RolPermisos
                    .Where(rp => rp.RolId == usuario.RolId)
                    .Select(rp => rp.Permiso.Nombre)
                    .ToListAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? string.Empty),
                    new Claim(ClaimTypes.Role, rol.Nombre ?? string.Empty),
                    new Claim("Email", usuario.Correo ?? string.Empty),
                };

                var secretKey = _configuration["Jwt:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("La clave JWT no está configurada");
                    return StatusCode(500, "Error de configuración JWT");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(1),
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
                    userImage = usuario.Imagen,
                    userPermissions = permisos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el inicio de sesión para {Email}", login.Email);
                return StatusCode(500, new
                {
                    message = "Error interno del servidor",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // =======================================================
        // RECUPERACIÓN: Enviar código a correo (sin cambios lógicos)
        // =======================================================
        [HttpPost("recuperar-contrasena")]
        public async Task<IActionResult> RecuperarContrasena([FromBody] RecuperarContrasenaDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Correo))
                return BadRequest(new { mensaje = "El correo es obligatorio." });

            var ok = await _authService.EnviarCodigoRecuperacionAsync(dto.Correo.Trim().ToLowerInvariant());
            if (!ok)
                return NotFound(new { mensaje = "Correo no encontrado o error al enviar el código." });

            return Ok(new { mensaje = "Código de recuperación enviado exitosamente" });
        }

        // =======================================================
        // RECUPERACIÓN: Verificar código (sin cambios lógicos)
        // =======================================================
        [HttpPost("verificar-codigo-recuperacion")]
        public async Task<IActionResult> VerificarCodigoRecuperacion([FromBody] VerificarCodigoRequest req)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var correo = req.Correo.Trim().ToLowerInvariant();
            var ok = await _authService.VerificarCodigoRecuperacionAsync(correo, req.Codigo.Trim());
            if (!ok) return BadRequest(new { message = "Código incorrecto o expirado" });

            return Ok(new { message = "Código verificado correctamente" });
        }

        // =============
        // RECUPERACIÓN
        // =============
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            try
            {
                if (!ModelState.IsValid) return ValidationProblem(ModelState);

                var correo = req.Correo.Trim().ToLowerInvariant();
                var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
                if (user == null) return NotFound(new { message = "Correo no registrado." });

                var codigoValido = await _authService.VerificarCodigoRecuperacionAsync(correo, req.Codigo.Trim());
                if (!codigoValido) return BadRequest(new { message = "Código inválido o expirado." });

                // HASH de la nueva contraseña
                user.Contrasena = _passwordHasher.HashPassword(user, req.NuevaContrasena);

                // Limpia el código para que no se reutilice
                user.CodigoRecuperacion = null;
                user.FechaExpiracionCodigo = null;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Contraseña actualizada correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reset-password para {Correo}", req?.Correo);
                return StatusCode(500, new { message = "Error interno del servidor." });
            }
        }
    }

    // ====== DTOs ======
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class VerificarCodigoRequest
    {
        public string Correo { get; set; }
        public string Codigo { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Correo { get; set; }
        public string NuevaContrasena { get; set; }
        public string Codigo { get; set; }
    }
}
