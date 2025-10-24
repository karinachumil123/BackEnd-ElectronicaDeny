using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEnd_ElectronicaDeny.Models; 

namespace BackEndElectronicaDeny.Models
{
    public class AperturaCaja
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        [Required, MaxLength(120)]
        public string CajeroNombre { get; set; } = string.Empty;

        [Required]
        public DateTime FechaAperturaUtc { get; set; } 

        // Monto de apertura
        [Required]
        [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
        public decimal MontoApertura { get; set; }

        // Notas opcionales
        [MaxLength(500)]
        public string? Notas { get; set; }
    }
}
