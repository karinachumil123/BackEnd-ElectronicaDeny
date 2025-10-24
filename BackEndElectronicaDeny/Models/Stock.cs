using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEndElectronicaDeny.Models
{
    public class Stock
    {
        // PK y FK = ProductoId (relación 1:1 con Productos)
        [Key, ForeignKey(nameof(Producto))]
        public int ProductoId { get; set; }

        public int StockDisponible { get; set; }      
        public int StockMinimo { get; set; }          

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioCompra { get; set; }     

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioVenta { get; set; }     

        [MaxLength(500)]
        public string? Imagen { get; set; }           

        [MaxLength(120)]
        public string NombreCategoria { get; set; } = string.Empty; 

        // NAV
        public virtual Productos Producto { get; set; } = null!;
    }
}
