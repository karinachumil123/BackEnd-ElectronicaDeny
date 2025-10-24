using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEnd_ElectronicaDeny.Models
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        [JsonIgnore] public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    }
}

