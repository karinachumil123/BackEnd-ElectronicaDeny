using System.ComponentModel.DataAnnotations;

namespace BackEnd_ElectronicaDeny.Models
{
    public class Estados
    {
        public Estados()
        {
            Usuarios = new HashSet<Usuario>();
        }

        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}
