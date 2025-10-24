using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEndElectronicaDeny.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        [ForeignKey("VentaId")]
        public virtual Venta? Venta { get; set; }

        [ForeignKey("ProductoId")]
        public virtual Productos? Producto { get; set; }
    }
}
