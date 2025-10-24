using BackEnd_ElectronicaDeny.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndElectronicaDeny.Models
{
    public class CierreCaja
    {
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(UsuarioId))]
        public virtual Usuario Usuario { get; set; } = null!;

        [Required, MaxLength(120)]
        public string CajeroNombre { get; set; } = string.Empty;

        [Required]
        public DateTime FechaCierreUtc { get; set; } = DateTime.UtcNow;

        public int? AperturaCajaId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(AperturaCajaId))]
        public virtual AperturaCaja? AperturaCaja { get; set; }

        [Required]
        public decimal BaseCaja { get; set; }

        public virtual ICollection<ClasificacionCaja> Clasificaciones { get; set; } = new List<ClasificacionCaja>();

        public virtual SaldosCaja Saldos { get; set; } = new SaldosCaja();
    }

    public class ClasificacionCaja
    {
        public int Id { get; set; }

        [MaxLength(80)]
        public string Denominacion { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public int Cantidad { get; set; }

        public decimal Subtotal { get; set; }

        public int CierreCajaId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(CierreCajaId))]
        public virtual CierreCaja CierreCaja { get; set; } = null!;
    }

    public class SaldosCaja
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
        public decimal AperturaCaja { get; set; }

        // Entradas (ventas en efectivo del día)
        public decimal Entradas { get; set; }
        public decimal Salidas { get; set; }

        // Subtotal (normalmente: AperturaCaja + Entradas)
        public decimal Subtotal { get; set; }

        // Total final (si aplicas otras salidas/ajustes, o igual al Subtotal)
        public decimal Total { get; set; }

        [Required]
        public int CierreCajaId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(CierreCajaId))]
        public virtual CierreCaja CierreCaja { get; set; } = null!;
    }
}
