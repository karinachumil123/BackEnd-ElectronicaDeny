using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEnd_ElectronicaDeny.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Contrasena { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public int edad { get; set; }
        public string? Imagen { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? UltimoInicioSesion { get; set; }
        public int EstadoId { get; set; }
        public int RolId { get; set; }
        public string? CodigoRecuperacion { get; set; }
        public DateTime? FechaExpiracionCodigo { get; set; }

        // Propiedades de navegación
        [JsonIgnore]
        [ForeignKey("EstadoId")]
        public virtual Estados Estado { get; set; }

        [JsonIgnore]
        [ForeignKey("RolId")]
        public virtual RolUsuario Rol { get; set; }
    }
}
