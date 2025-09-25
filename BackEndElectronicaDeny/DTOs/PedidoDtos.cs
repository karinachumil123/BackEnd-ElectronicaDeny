using System.ComponentModel.DataAnnotations;

namespace BackEndElectronicaDeny.Dtos
{
    public class DetallePedidoDto
    {
        [Required] public int ProductoId { get; set; }
        [Required, Range(1, 100000)] public int Cantidad { get; set; }
        [Required, Range(0, 999999)] public decimal PrecioUnitario { get; set; }
    }

    public class CrearPedidoDto
    {
        [Required, MaxLength(50)] public string? NumeroPedido { get; set; }
        [Required, MaxLength(150)] public string? NombrePedido { get; set; }
        [Required] public DateTime FechaPedido { get; set; }
        [Required] public int ProveedorId { get; set; }
        [Required] public int EstadoPedidoId { get; set; } // 1..5
        [MaxLength(500)] public string? Descripcion { get; set; }
        public decimal? TotalPedido { get; set; }
        [MinLength(1)] public List<DetallePedidoDto> Detalles { get; set; } = new();
    }

    public class ActualizarPedidoDto : CrearPedidoDto { }

    public class EnviarAInventarioDto
    {
        public string? Observacion { get; set; }
    }
}
