using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BackEnd_ElectronicaDeny.Models
{
    public enum UsuarioEstado
    {
        Activo = 1,
        Inactivo = 2
    }

    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Nombre { get; set; }
        [Required, MaxLength(50)]
        public string Apellido { get; set; }
        [Required, EmailAddress, MaxLength(120)]
        public string Correo { get; set; }
        [MaxLength(12)]
        public string Telefono { get; set; }
        // Almacena HASH, no el texto plano
        [JsonIgnore]
        [MaxLength(256)]
        [Column(TypeName = "varchar(256)")] 
        public string Contrasena { get; set; }
        [Column(TypeName = "date")]
        public DateTime? FechaNacimiento { get; set; }
        public int edad { get; set; }
        public string? Imagen { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? UltimoInicioSesion { get; set; }

        public int Estado { get; set; }  

        // Mantienes Rol como antes
        public int RolId { get; set; }

        public string? CodigoRecuperacion { get; set; }
        public DateTime? FechaExpiracionCodigo { get; set; }

        [JsonIgnore]
        public virtual RolUsuario Rol { get; set; }
    }
}
