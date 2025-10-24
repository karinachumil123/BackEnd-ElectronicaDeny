using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd_ElectronicaDeny.DTOs
{
    public class UsuarioUpdateDto
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [Required, StringLength(50)]
        public string Apellido { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; }

        [Phone]
        public string Telefono { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FechaNacimiento { get; set; }
        public int edad { get; set; }
        public string Imagen { get; set; }

        [Required, Range(1, 2)]  
        public int Estado { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int RolId { get; set; }

        public string? NuevaContrasena { get; set; }  // si alguna vez necesitas reset
    }
}
