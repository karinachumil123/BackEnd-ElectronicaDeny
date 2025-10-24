using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackEndElectronicaDeny.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext db, ILogger<DashboardController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /api/dashboard/resumen
        [HttpGet("resumen")]
        public async Task<ActionResult<DashboardResumenDto>> GetResumen()
        {
            try
            {
                // ====== Rango hoy (zona GT) ======
                var tz = GetGtTz();
                var hoyLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;
                var (fromHoyUtc, toHoyUtc) = GetUtcRangeForLocalDate(hoyLocal);

                // ====== Rango mes (zona GT) ======
                var primerDiaMesLocal = new DateTime(hoyLocal.Year, hoyLocal.Month, 1);
                var primerDiaSiguienteLocal = primerDiaMesLocal.AddMonths(1);
                var fromMesUtc = TimeZoneInfo.ConvertTimeToUtc(primerDiaMesLocal, tz);
                var toMesUtc = TimeZoneInfo.ConvertTimeToUtc(primerDiaSiguienteLocal, tz);

                // ====== Ventas del día ======
                var ventasHoy = await _db.Ventas
                    .AsNoTracking()
                    .Where(v => v.FechaVenta >= fromHoyUtc && v.FechaVenta < toHoyUtc)
                    .SumAsync(v => (decimal?)v.Total) ?? 0m;

                // ====== Ventas del mes ======
                var ventasMes = await _db.Ventas
                    .AsNoTracking()
                    .Where(v => v.FechaVenta >= fromMesUtc && v.FechaVenta < toMesUtc)
                    .SumAsync(v => (decimal?)v.Total) ?? 0m;

                // ====== Últimas 5 ventas (con sus productos/cantidades) ======
                var ultimasVentasRaw = await _db.Ventas
                    .AsNoTracking()
                    .OrderByDescending(v => v.FechaVenta)
                    .Include(v => v.DetallesVenta)
                        .ThenInclude(d => d.Producto)
                    .Take(5)
                    .ToListAsync();

                var ultimasVentas = ultimasVentasRaw.Select(v => new VentaResumenDto
                {
                    Id = v.Id,
                    FechaVenta = v.FechaVenta,
                    Total = v.Total,
                    Productos = v.DetallesVenta?
                        .Select(d => new ProductoCantDto
                        {
                            Nombre = d.Producto != null ? (d.Producto.Nombre ?? "Producto") : "Producto",
                            Cantidad = d.Cantidad
                        })
                        .ToList() ?? new List<ProductoCantDto>()
                })
                .ToList();

                // ====== Producto más vendido (por cantidad) ======
                // Puedes limitar por periodo si quieres (p.ej. mes). Aquí: histórico completo.
                var masVendidoQ = await _db.DetalleVenta
                    .AsNoTracking()
                    .Include(d => d.Producto)
                    .GroupBy(d => d.Producto!.Nombre)
                    .Select(g => new
                    {
                        Nombre = g.Key ?? "Producto",
                        Cantidad = g.Sum(x => x.Cantidad)
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .FirstOrDefaultAsync();

                ProductoCantDto? productoMasVendido = null;
                if (masVendidoQ != null)
                {
                    productoMasVendido = new ProductoCantDto
                    {
                        Nombre = masVendidoQ.Nombre,
                        Cantidad = masVendidoQ.Cantidad
                    };
                }

                // ====== Productos con bajo stock ======
                var productosBajoStock = await _db.Stock
                    .AsNoTracking()
                    .Include(s => s.Producto)
                    .Where(s => s.StockDisponible <= s.StockMinimo)
                    .Select(s => new ProductoStockDto
                    {
                        Nombre = s.Producto!.Nombre ?? "Producto",
                        Stock = s.StockDisponible
                    })
                    .ToListAsync();

                var dto = new DashboardResumenDto
                {
                    VentasHoy = ventasHoy,
                    VentasMes = ventasMes,
                    ProductoMasVendido = productoMasVendido,
                    ProductosBajoStock = productosBajoStock,
                    UltimasVentas = ultimasVentas
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al armar resumen de dashboard");
                return StatusCode(500, "Error interno al obtener el resumen del dashboard");
            }
        }

        // ===== Helpers de zona horaria (mismo patrón que tu VentaController) =====
        private static TimeZoneInfo GetGtTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala"); }     // Linux
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); } // Windows
        }

        private static (DateTime fromUtc, DateTime toUtc) GetUtcRangeForLocalDate(DateTime fechaLocal)
        {
            var tz = GetGtTz();
            var fromLocal = fechaLocal.Date;
            var toLocal = fromLocal.AddDays(1);
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(toLocal, tz);
            return (fromUtc, toUtc);
        }
    }
}
