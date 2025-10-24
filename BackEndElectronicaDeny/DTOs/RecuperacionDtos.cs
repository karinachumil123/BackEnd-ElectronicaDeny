using System.ComponentModel.DataAnnotations;

namespace BackEnd_ElectronicaDeny.DTOs
{
    public class RecuperarContrasenaRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;
    }

    public class VerificarCodigoRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Codigo { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required]
        public string Codigo { get; set; } = string.Empty;
    }
}
