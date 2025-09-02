using System.ComponentModel.DataAnnotations;

namespace BackEnd_ElectronicaDeny.DTOs
{
    public class UsuarioUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Correo { get; set; }

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int edad { get; set; }
        public string Imagen { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un estado válido")]
        public int EstadoId { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido")]
        public int RolId { get; set; }

        // Nota: No incluir campo Contrasena aquí
    }
}
