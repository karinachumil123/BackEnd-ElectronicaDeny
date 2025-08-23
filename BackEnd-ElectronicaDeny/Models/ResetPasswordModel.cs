namespace BackEnd_ElectronicaDeny.Models
{
    public class ResetPasswordModel
    {
        public string Correo { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public string NewContrasena { get; set; } = string.Empty;
    }
}
