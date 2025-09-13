using BackEnd_ElectronicaDeny.Models;
using System.ComponentModel.DataAnnotations;

namespace BackEndElectronicaDeny.Models
{
    public class EstadoProductoFisico
    {
        public EstadoProductoFisico()
        {
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}

