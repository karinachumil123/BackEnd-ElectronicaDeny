using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using BackEndElectronicaDeny.DTOs.TiendaKeytlin.Server.DTOs;
using BackEndElectronicaDeny.Models;
using BackEndElectronicaDeny.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackEndElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VentaController> _logger;
        private readonly InventoryService _inv;

        public VentaController(
            AppDbContext context,
            ILogger<VentaController> logger,
            InventoryService inv)
        {
            _context = context;
            _logger = logger;
            _inv = inv;
        }

        // =========================
        // GET: api/venta
        // =========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaResponseDto>>> GetVentas()
        {
            try
            {
                var ventas = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Vendedor)
                    .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                    .OrderByDescending(v => v.FechaVenta)
                    .ToListAsync();

                var resp = ventas.Select(MapVentaToResponseDto).ToList();
                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ventas");
                return StatusCode(500, "Error interno al obtener ventas");
            }
        }

        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetTotalDelDia([FromQuery] DateTime? fecha)
        {
            if (fecha is null) return BadRequest("fecha requerida (yyyy-MM-dd)");

            var tz = TimeZoneInfo.Local;
            var fromLocal = fecha.Value.Date;
            var toLocal = fromLocal.AddDays(1);

            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(toLocal, tz);

            var total = await _context.Ventas
                .Where(v => v.FechaVenta >= fromUtc && v.FechaVenta < toUtc)
                .SumAsync(v => (decimal?)v.Total) ?? 0m;

            return Ok(total);
        }

        // =========================
        // GET: api/venta/{id}
        // =========================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VentaResponseDto>> GetVenta(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Vendedor)
                    .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null) return NotFound($"No se encontró la venta con ID {id}");

                return Ok(MapVentaToResponseDto(venta));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener venta {Id}", id);
                return StatusCode(500, "Error interno al obtener la venta");
            }
        }

        // =========================
        // GET: api/venta/recibo/{id}
        // =========================
        [HttpGet("recibo/{id:int}")]
        public async Task<ActionResult<ReciboVentaDto>> GetReciboVenta(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Vendedor)
                    .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                    return NotFound($"No se encontró la venta con ID {id}");

                var empresa = await _context.Empresa.AsNoTracking().FirstOrDefaultAsync();
                var direccionTienda = empresa?.Direccion ?? "Dirección no configurada";
                var telefonoEmpresa = empresa?.Telefono ?? "—";

                // Nombre + apellido del vendedor
                var nombreV = (venta.Vendedor?.Nombre ?? string.Empty).Trim();
                var apellidoV = (venta.Vendedor?.Apellido ?? string.Empty).Trim();
                var nombreCompletoVendedor = string.IsNullOrWhiteSpace(apellidoV) ? nombreV : $"{nombreV} {apellidoV}".Trim();
                if (string.IsNullOrWhiteSpace(nombreCompletoVendedor)) nombreCompletoVendedor = "No disponible";

                string? clienteNit = null;

                if (venta.ClienteId > 0)
                {
                    clienteNit = await _context.Clientes
                        .AsNoTracking()
                        .Where(c => c.Id == venta.ClienteId)
                        .Select(c => c.NIT)          
                        .FirstOrDefaultAsync();
                }


                var recibo = new ReciboVentaDto
                {
                    VentaId = venta.Id,
                    FechaVenta = venta.FechaVenta,

                    DireccionTienda = direccionTienda,
                    TelefonoEmpresa = telefonoEmpresa,

                    NombreVendedor = nombreCompletoVendedor,
                    EmailVendedor = venta.Vendedor?.Correo ?? "No disponible",
                    TelefonoVendedor = venta.Vendedor?.Telefono ?? "No disponible",
                    
                    ClienteId = venta.ClienteId,
                    ClienteNombre = venta.ClienteNombre ?? "Cliente",
                    ClienteTelefono = venta.ClienteTelefono ?? "",
                    ClienteDireccion = venta.ClienteDireccion,
                    ClienteNit = clienteNit, // ⬅️ IMPORTANTE

                    Subtotal = venta.Subtotal,
                    Total = venta.Total,
                    MontoRecibido = venta.MontoRecibido,
                    Cambio = venta.Cambio,

                    Productos = venta.DetallesVenta?.Select(d => new DetalleVentaResponseDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto?.Nombre ?? "Producto",
                        CodigoProducto = d.Producto?.CodigoProducto ?? "",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Subtotal
                    }).ToList() ?? new List<DetalleVentaResponseDto>()
                };

                return Ok(recibo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar recibo para venta {Id}", id);
                return StatusCode(500, "Error interno al generar el recibo");
            }
        }

        private static TimeZoneInfo GetGtTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala"); }     // Linux
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); } // Windows
        }

        /// Rango UTC que cubre el día local de 'fechaLocal' (00:00–24:00)
        private static (DateTime fromUtc, DateTime toUtc) GetUtcRangeForLocalDate(DateTime fechaLocal)
        {
            var tz = GetGtTz();
            var fromLocal = fechaLocal.Date;                 // 00:00 local
            var toLocal = fromLocal.AddDays(1);            // 00:00 del día siguiente local
            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(toLocal, tz);
            return (fromUtc, toUtc);
        }


        // =========================
        // POST: api/venta
        // =========================
        [HttpPost]
        public async Task<ActionResult<VentaResponseDto>> PostVenta([FromBody] VentaCreateDto ventaDto)
        {
            if (ventaDto == null) return BadRequest("Datos de venta vacíos.");
            if (ventaDto.DetallesVenta == null || ventaDto.DetallesVenta.Count == 0)
                return BadRequest("Debe incluir al menos un item en la venta.");

            try
            {
                // ============================================
                // Apertura requerida (día actual en hora local)
                // ============================================

                // Usa la zona de Guatemala (cae en Central Standard Time en Windows)
                var tz = GetGtTz();

                // "Hoy" en horario local (sin hora)
                var hoyLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;

                // Rango [hoy 00:00 local, mañana 00:00 local) convertido a UTC
                var (fromUtc, toUtc) = GetUtcRangeForLocalDate(hoyLocal);

                var hayAperturaHoy = await _context.AperturasCaja
                    .AsNoTracking()
                    .AnyAsync(a =>
                        a.FechaAperturaUtc >= fromUtc && a.FechaAperturaUtc < toUtc
                    );

                if (!hayAperturaHoy)
                {
                    // 409 Conflict: no permitir ventas sin apertura del día
                    return StatusCode(409, new
                    {
                        message = "No hay apertura de caja para hoy. Registre la apertura para poder realizar ventas."
                    });
                }
                

                // Vendedor
                var vendedor = await _context.Usuarios.FindAsync(ventaDto.VendedorId);
                if (vendedor == null)
                    return BadRequest($"El vendedor con ID {ventaDto.VendedorId} no existe");

                // Validar items + calcular subtotal
                decimal subtotal = 0m;
                var detallesAInsertar = new List<DetalleVenta>();

                foreach (var det in ventaDto.DetallesVenta)
                {
                    var producto = await _context.Productos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == det.ProductoId);

                    if (producto == null)
                        return BadRequest($"El producto con ID {det.ProductoId} no existe");
                    if (det.Cantidad <= 0)
                        return BadRequest($"La cantidad del producto {det.ProductoId} debe ser mayor a cero");
                    if (det.PrecioUnitario < 0)
                        return BadRequest($"Precio inválido para el producto {det.ProductoId}");

                    var sub = det.Cantidad * det.PrecioUnitario;
                    subtotal += sub;

                    detallesAInsertar.Add(new DetalleVenta
                    {
                        ProductoId = det.ProductoId,
                        Cantidad = det.Cantidad,
                        PrecioUnitario = det.PrecioUnitario,
                        Subtotal = sub
                    });
                }

                var total = subtotal;

                // Cambio / monto recibido
                if (ventaDto.MontoRecibido < 0)
                    return BadRequest("El monto recibido no puede ser negativo.");
                var cambio = ventaDto.MontoRecibido - total;
                if (cambio < 0) cambio = 0m;

                // Cliente (opcional)
                int? clienteId = (ventaDto.ClienteId.HasValue && ventaDto.ClienteId.Value > 0)
                    ? ventaDto.ClienteId.Value
                    : (int?)null;

                string? cliNombre = null;
                string? cliTel = null;
                string? cliDir = null;

                if (clienteId.HasValue)
                {
                    var cliente = await _context.Clientes.FindAsync(clienteId.Value);
                    if (cliente == null)
                        return BadRequest($"El cliente con ID {clienteId.Value} no existe");

                    cliNombre = cliente.NombreCompleto;
                    cliTel = cliente.Telefono;
                    cliDir = cliente.Direccion;
                }

                using var tx = await _context.Database.BeginTransactionAsync();

                // Crear venta
                var venta = new Venta
                {
                    FechaVenta = DateTime.UtcNow,
                    Subtotal = subtotal,
                    Total = total,
                    MontoRecibido = ventaDto.MontoRecibido,
                    Cambio = cambio,
                    Observaciones = string.IsNullOrWhiteSpace(ventaDto.Observaciones) ? null : ventaDto.Observaciones.Trim(),
                    VendedorId = ventaDto.VendedorId,

                    // snapshot de cliente (todo opcional)
                    ClienteId = clienteId,        // <- null si no hay cliente
                    ClienteNombre = cliNombre,
                    ClienteTelefono = cliTel,
                    ClienteDireccion = cliDir
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // Insertar detalles
                foreach (var d in detallesAInsertar)
                {
                    d.VentaId = venta.Id;
                    _context.DetalleVenta.Add(d);
                }
                await _context.SaveChangesAsync();

                // Descontar stock por venta (InventoryService NO guarda)
                var movimientos = detallesAInsertar
                    .GroupBy(x => x.ProductoId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Cantidad));
                await _inv.DescontarPorVentaAsync(movimientos);

                // Guardar cambios de stock y confirmar
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // Respuesta con venta completa
                var ventaCompleta = await _context.Ventas
                    .AsNoTracking()
                    .Include(v => v.Vendedor)
                    .Include(v => v.DetallesVenta).ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == venta.Id);

                if (ventaCompleta == null)
                    return StatusCode(500, "No se pudo recuperar la venta creada.");

                var resp = MapVentaToResponseDto(ventaCompleta);
                return CreatedAtAction(nameof(GetVenta), new { id = venta.Id }, resp);
            }
            catch (InvalidOperationException invEx)
            {
                _logger.LogWarning(invEx, "Inventario: error de negocio al crear venta");
                return BadRequest(invEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la venta");
                return StatusCode(500, "Error interno al crear la venta");
            }
        }


        // =========================
        // Mapper
        // =========================
        private static VentaResponseDto MapVentaToResponseDto(Venta v)
        {
            return new VentaResponseDto
            {
                Id = v.Id,
                FechaVenta = v.FechaVenta,
                Subtotal = v.Subtotal,
                Total = v.Total,
                MontoRecibido = v.MontoRecibido,
                Cambio = v.Cambio,
                Observaciones = v.Observaciones,
                Cantidad = v.DetallesVenta?.Sum(d => d.Cantidad) ?? 0,

                Vendedor = v.Vendedor != null
                    ? new VendedorDto
                    {
                        Id = v.Vendedor.Id,
                        Nombre = v.Vendedor.Nombre ?? "Sin nombre",
                        Apellido = v.Vendedor.Apellido ?? "Sin apellido",
                        Email = v.Vendedor.Correo ?? "",
                        Telefono = v.Vendedor.Telefono ?? ""
                    }
                    : new VendedorDto(),

                // Si tu DTO tiene ClienteId como int (no-null), mapea 0 cuando no hay cliente
                ClienteId = v.ClienteId,
                ClienteNombre = v.ClienteNombre ?? string.Empty,
                ClienteTelefono = v.ClienteTelefono ?? string.Empty,
                ClienteDireccion = v.ClienteDireccion,

                DetallesVenta = v.DetallesVenta?.Select(d => new DetalleVentaResponseDto
                {
                    Id = d.Id,
                    ProductoId = d.ProductoId,
                    NombreProducto = d.Producto?.Nombre ?? "Producto",
                    CodigoProducto = d.Producto?.CodigoProducto ?? "",
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList() ?? new List<DetalleVentaResponseDto>()
            };
        }
    }
}
