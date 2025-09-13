using BackEnd_ElectronicaDeny.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndElectronicaDeny.Models
{
    public class Productos
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El código del producto es obligatorio")]
        public string CodigoProducto { get; set; }

        public string MarcaProducto { get; set; }

        [Required(ErrorMessage = "El precio de adquisición es obligatorio")]
        [Range(0, 999999, ErrorMessage = "El precio de adquisición debe estar entre 0 y 999999")]
        public decimal PrecioAdquisicion { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(0, 999999, ErrorMessage = "El precio de venta debe estar entre 0 y 999999")]
        public decimal PrecioVenta { get; set; }
        public int EstadoId { get; set; }

        public string? Descripcion { get; set; }

        public string? Imagen { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int ProveedorId { get; set; }

        [JsonIgnore]
        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [JsonIgnore]
        [ForeignKey("ProveedorId")]
        public virtual Proveedor? Proveedor { get; set; }

        [JsonIgnore]
        [ForeignKey("EstadoId")]
        public virtual Estados Estado { get; set; }

        public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new HashSet<DetallePedido>();
    }
}
