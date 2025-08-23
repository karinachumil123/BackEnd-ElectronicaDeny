using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.DTOs;
using BackEnd_ElectronicaDeny.Models;
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
    public class AuthService 
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


    }
}
