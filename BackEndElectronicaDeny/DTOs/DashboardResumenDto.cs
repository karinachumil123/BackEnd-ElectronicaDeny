using System.Collections.Generic;

namespace BackEndElectronicaDeny.DTOs
{
    public class DashboardResumenDto
    {
        public decimal VentasHoy { get; set; }
        public decimal VentasMes { get; set; }
        public ProductoCantDto? ProductoMasVendido { get; set; }
        public List<ProductoStockDto> ProductosBajoStock { get; set; } = new();
        public List<VentaResumenDto> UltimasVentas { get; set; } = new();
    }
}
