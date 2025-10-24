using System.ComponentModel.DataAnnotations;

namespace BackEndElectronicaDeny.Models
{
    public class Clientes
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string NombreCompleto { get; set; } = null!;        
            

        [EmailAddress, MaxLength(120)]
        public string? Correo { get; set; }

        [Required, MaxLength(25)]
        public string Telefono { get; set; } = null!;      

        [MaxLength(14)]
        public string? NIT { get; set; }                  

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(60)]
        public string? Municipio { get; set; }

        [MaxLength(60)]
        public string? Departamento { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public int Estado { get; set; } = 1;
        public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
