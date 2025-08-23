using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEnd_ElectronicaDeny.Models
{
    public class RolUsuario
    {

        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        // Ignorar la serialización de estas colecciones cuando se cree un rol
        [JsonIgnore]
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        [JsonIgnore]
        public ICollection<Permiso> Permisos { get; set; } = new List<Permiso>();
    }
}
