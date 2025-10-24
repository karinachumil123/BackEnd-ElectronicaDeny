using BackEnd_ElectronicaDeny.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndElectronicaDeny.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [Required] public DateTime FechaVenta { get; set; }
        [Required] public decimal Subtotal { get; set; }
        [Required] public decimal Total { get; set; }
        [Required] public decimal MontoRecibido { get; set; }
        [Required] public decimal Cambio { get; set; }

        public string? Observaciones { get; set; }

        // Vendedor
        [Required] public int VendedorId { get; set; }
        [ForeignKey(nameof(VendedorId))] public virtual Usuario? Vendedor { get; set; }

        // ===== Cliente OPCIONAL =====
        public int? ClienteId { get; set; }                    // <— ahora es nullable
        [ForeignKey(nameof(ClienteId))] public virtual Clientes? Cliente { get; set; }

        // Snapshot de cliente (también opcional)
        [MaxLength(200)] public string? ClienteNombre { get; set; }
        [MaxLength(25)] public string? ClienteTelefono { get; set; }
        [MaxLength(200)] public string? ClienteDireccion { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}

