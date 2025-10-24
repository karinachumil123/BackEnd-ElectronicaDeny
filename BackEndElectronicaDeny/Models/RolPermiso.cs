using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd_ElectronicaDeny.Models
{
    public class RolPermiso
    {
        public int RolId { get; set; }
        public int PermisoId { get; set; }

        // Relaciones
        public virtual RolUsuario Rol { get; set; }
        public virtual Permiso Permiso { get; set; }
    }
}
