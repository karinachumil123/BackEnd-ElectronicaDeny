namespace BackEndElectronicaDeny.DTOs
{
    public class StockListItemDto
    {
        public int ProductoId { get; set; }
        public string? CodigoProducto { get; set; }
        public string Nombre { get; set; } = "";
        public int StockDisponible { get; set; }
        public int StockMinimo { get; set; }
        public string NombreCategoria { get; set; } = "";
        public decimal PrecioVenta { get; set; }
        public decimal PrecioCompra { get; set; }
        public string? Imagen { get; set; }
    }

    public class FromPedidoItemDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCompra { get; set; }
    }

    public class FromPedidoDto
    {
        public int PedidoId { get; set; }
        public List<FromPedidoItemDto> Items { get; set; } = new();
    }

    public class UpdateStockDto
    {
        public int StockDisponible { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioVenta { get; set; }
    }
}
