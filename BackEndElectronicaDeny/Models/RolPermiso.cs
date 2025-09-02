using System.ComponentModel.DataAnnotations;

namespace BackEnd_ElectronicaDeny.Models
{
    public class RolPermiso
    {
        [Key]
        public int Id { get; set; }
        public int RolId { get; set; }
        public int PermisoId { get; set; }

        // Relaciones
        public virtual RolUsuario Rol { get; set; }
        public virtual Permiso Permiso { get; set; }
    }
}
