using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd_ElectronicaDeny.DTOs
{
    public class UsuarioCreateDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Imagen { get; set; }
        [Column(TypeName = "date")]
        public DateTime? FechaNacimiento { get; set; }
        public int edad { get; set; }

        // Opcional: si decides que BACKEND genere la contraseña, deja esto fuera.
        public string? Contrasena { get; set; }

        public string Telefono { get; set; }

        public int Estado { get; set; }

        public int RolId { get; set; }
        // FechaCreacion la setea el servidor
    }
}
