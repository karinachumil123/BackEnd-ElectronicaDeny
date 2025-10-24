using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.Models;
using System.Security.Claims;
using BackEndElectronicaDeny.DTOs;

[ApiController]
[Route("api/cierrecaja")]
[Authorize]
public class CierresCajaController : ControllerBase
{
    private readonly AppDbContext _db;
    public CierresCajaController(AppDbContext db) => _db = db;

    private int? TryGetUserId()
    {
        var s = User.FindFirst("sub")?.Value
              ?? User.FindFirst("id")?.Value
              ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(s, out var id) ? id : null;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetById(int id)
    {
        var c = await _db.CierresCaja
            .AsNoTracking()
            .Include(x => x.Clasificaciones)
            .Include(x => x.Saldos)
            .Where(x => x.Id == id)
            .Select(x => new {
                x.Id,
                x.CajeroNombre,
                x.FechaCierreUtc,
                x.BaseCaja,
                Clasificaciones = x.Clasificaciones.Select(c => new {
                    c.Denominacion,
                    c.Valor,
                    c.Cantidad,
                    c.Subtotal
                }),
                Saldos = new
                {
                    x.Saldos.Descripcion,
                    x.Saldos.AperturaCaja,
                    x.Saldos.Entradas,
                    x.Saldos.Salidas,
                    x.Saldos.Subtotal,
                    x.Saldos.Total
                }
            })
            .FirstOrDefaultAsync();

        return c is null ? NotFound() : Ok(c);
    }


    [HttpPost]
    public async Task<ActionResult<CierreCaja>> Crear([FromBody] CierreCajaCreateDto dto)
    {
        var userId = TryGetUserId();
        if (userId is null) return Unauthorized("Token sin id de usuario.");

        var user = await _db.Usuarios.FindAsync(userId.Value);
        if (user is null) return Unauthorized("Usuario no encontrado.");

        // Rango del día local GT
        TimeZoneInfo tz;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala"); }
        catch { tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); }

        var hoyLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(hoyLocal, tz);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(hoyLocal.AddDays(1), tz);

        // Apertura del día (obligatoria)
        var apertura = await _db.AperturasCaja
            .Where(a => a.FechaAperturaUtc >= fromUtc && a.FechaAperturaUtc < toUtc)
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync();

        if (apertura is null) return BadRequest("No existe Apertura para la fecha de cierre.");

        // Entradas = ventas del día
        var ventasDelDia = await _db.Ventas
            .Where(v => v.FechaVenta >= fromUtc && v.FechaVenta < toUtc)
            .SumAsync(v => (decimal?)v.Total) ?? 0m;

        // Salidas = compras/pedidos del día
        var salidasDelDia = await _db.Pedidos
            .Where(p => p.FechaPedido >= fromUtc && p.FechaPedido < toUtc)
            .SumAsync(p => (decimal?)p.TotalPedido) ?? 0m;

        // Suma de clasificaciones (si no vienen → 0 y lista vacía)
        var clasifs = (dto.Clasificaciones ?? new List<ClasificacionDto>()).Select(c => new {
            Denominacion = c.Denominacion ?? "",
            Valor = c.Valor,
            Cantidad = c.Cantidad,
            Subtotal = c.Cantidad * c.Valor
        }).ToList();

        var sumaClasificaciones = clasifs.Sum(c => c.Subtotal);

        // Total según regla (puede ser negativo)
        var total = apertura.MontoApertura + ventasDelDia - salidasDelDia;

        // Validación: Subtotal vs Total
        const decimal EPS = 0.01m;
        var cuadran = Math.Abs(total - sumaClasificaciones) <= EPS;

        if (!cuadran)
        {
            // Descripción obligatoria con mínimo 3 palabras
            var desc = (dto.Descripcion ?? "").Trim();
            int palabras = string.IsNullOrEmpty(desc) ? 0
                         : desc.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            if (palabras < 3)
                return BadRequest("El Subtotal no cuadra con el Total. Debe ingresar una descripción con al menos 3 palabras.");
        }

        var cierre = new CierreCaja
        {
            UsuarioId = user.Id,
            CajeroNombre = $"{user.Nombre} {user.Apellido}".Trim(),
            FechaCierreUtc = dto.FechaCierreUtc ?? DateTime.UtcNow,
            AperturaCajaId = apertura.Id,
            BaseCaja = apertura.MontoApertura,
            Clasificaciones = clasifs.Select(c => new ClasificacionCaja
            {
                Denominacion = c.Denominacion,
                Valor = c.Valor,
                Cantidad = c.Cantidad,
                Subtotal = c.Subtotal
            }).ToList(),
            Saldos = new SaldosCaja
            {
                Descripcion = dto.Descripcion,
                AperturaCaja = apertura.MontoApertura,
                Entradas = ventasDelDia,
                Salidas = salidasDelDia,
                Subtotal = sumaClasificaciones, // suma clasif
                Total = total                // apertura + ventas - salidas
            }
        };

        _db.CierresCaja.Add(cierre);
        await _db.SaveChangesAsync();

        return Created($"/api/cierrecaja/{cierre.Id}", new { id = cierre.Id });
    }

}

