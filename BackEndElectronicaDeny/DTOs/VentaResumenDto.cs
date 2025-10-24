using System;
using System.Collections.Generic;

namespace BackEndElectronicaDeny.DTOs
{
    public class VentaResumenDto
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public List<ProductoCantDto> Productos { get; set; } = new();
    }
}
