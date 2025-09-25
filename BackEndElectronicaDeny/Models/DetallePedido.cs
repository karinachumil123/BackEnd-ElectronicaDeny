using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndElectronicaDeny.Models
{
    public class DetallePedido
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Pedido))]
        public int PedidoId { get; set; }  // FK -> Pedido.Id

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, 100000)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;

        // Navegaciones
        [JsonIgnore]
        public virtual Pedido? Pedido { get; set; }

        [JsonIgnore]
        public virtual Productos? Producto { get; set; }
    }
}
