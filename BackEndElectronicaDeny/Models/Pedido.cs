using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Models
{
    // Índice único sobre NumeroPedido
    [Index(nameof(NumeroPedido), IsUnique = true)]
    [Index(nameof(FechaPedido))]
    [Index(nameof(EstadoPedidoId))]
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El número de pedido es obligatorio")]
        [MaxLength(50)]
        public string NumeroPedido { get; set; } = null!; // ÚNICO, obligatorio

        [Required(ErrorMessage = "El nombre del pedido es obligatorio")]
        [MaxLength(150)]
        public string NombrePedido { get; set; } = null!;

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        [ForeignKey(nameof(Proveedor))]
        public int ProveedorId { get; set; }

        // Estado viene del front como número (1=Pendiente, 2=Enviado, 3=Recibido, 4=Cancelado)
        [Required(ErrorMessage = "El estado del pedido es obligatorio")]
        public int EstadoPedidoId { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Range(0, 9999999, ErrorMessage = "El total debe estar entre 0 y 9,999,999")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPedido { get; set; }

        // Navegaciones
        public virtual Proveedor? Proveedor { get; set; }

        public virtual ICollection<DetallePedido> Detalles { get; set; } = new HashSet<DetallePedido>();
    }
}
