using System.ComponentModel.DataAnnotations;

namespace BackEnd_ElectronicaDeny.Models
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
