using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.DTOs;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackEnd_ElectronicaDeny.Services
{
    public interface IAuthService
    {
        Task<bool> EnviarCodigoRecuperacionAsync(string correo);
        Task<bool> VerificarCodigoRecuperacionAsync(string correo, string codigo);
        Task<bool> ResetPasswordAsync(ResetPasswordModel model);
    }
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }
        public string Authenticate(LoginDTO loginDTO)
        {
            var usuario = _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Correo == loginDTO.Email && u.Contrasena == loginDTO.Password);

            if (usuario == null)
            {
                return null;
            }

            var rol = _context.Roles.Find(usuario.RolId);
            if (rol == null)
            {
                throw new InvalidOperationException("Rol no encontrado para el usuario");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, usuario.Correo),
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Role, rol.Nombre)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> EnviarCodigoRecuperacionAsync(string correo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null)
            {
                return false;
            }

            var codigo = new Random().Next(100000, 999999).ToString();
            usuario.CodigoRecuperacion = codigo;
            usuario.FechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            var emailModel = new EmailModel
            {
                Email = correo,
                Subject = "Código de recuperación de contraseña",
                Message = $"Tu código de recuperación es: {codigo}. Este código expirará en 15 minutos."
            };

            await _emailService.SendEmailAsync(emailModel);
            return true;
        }

        public async Task<bool> VerificarCodigoRecuperacionAsync(string correo, string codigo)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null || usuario.CodigoRecuperacion != codigo || usuario.FechaExpiracionCodigo < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordModel model)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == model.Correo && u.CodigoRecuperacion == model.VerificationCode);

            if (usuario == null)
                return false;

            usuario.Contrasena = model.NewContrasena;
            usuario.CodigoRecuperacion = null;
            usuario.FechaExpiracionCodigo = null;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
