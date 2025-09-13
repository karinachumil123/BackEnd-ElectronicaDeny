using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndElectronicaDeny.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Required]
        public int ProductoId { get; set; }

        [Range(0, 9999999, ErrorMessage = "El total debe estar entre 0 y 9,999,999")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "El estado del pedido es obligatorio")]
        public int EstadoPedidoId { get; set; }

        public virtual Proveedor? Proveedor { get; set; }

        //public virtual EstadoPedido? EstadoPedido { get; set; }
        [ForeignKey("ProductoId")]
        public virtual Productos Producto { get; set; }

        public virtual ICollection<DetallePedido> Detalles { get; set; } = new HashSet<DetallePedido>();
    }
}
