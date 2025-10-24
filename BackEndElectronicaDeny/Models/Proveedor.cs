using BackEnd_ElectronicaDeny.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndElectronicaDeny.Models
{
    public class Proveedor
    {
        public Proveedor()
        {
            Productos = new HashSet<Productos>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty; 

        [Required]
        public string NombreContacto { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;
        public string? TelefonoContacto { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public string? Descripcion { get; set; }

        [Required]
        public int EstadoId { get; set; }

        [JsonIgnore]
        public virtual ICollection<Productos> Productos { get; set; }
    }
}

