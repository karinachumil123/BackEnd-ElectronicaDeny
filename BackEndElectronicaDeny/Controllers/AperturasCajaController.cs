using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.Models;
using BackEndElectronicaDeny.DTOs;

namespace BackEndElectronicaDeny.Controllers
{
    [ApiController]
    [Route("api/aperturacaja")]
    [Authorize]
    public class AperturasCajaController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AperturasCajaController(AppDbContext db)
        {
            _db = db;
        }

        // ===== Helpers =====
        private int GetUserId()
        {
            var s = User.FindFirst("sub")?.Value
                     ?? User.FindFirst("id")?.Value
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(s, out var id) ? id : 1; // fallback solo en dev
        }

        private static TimeZoneInfo GetGtTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala"); }     // Linux
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); } // Windows
        }

        /// Convierte una fecha local (yyyy-MM-dd) al rango UTC [00:00, 24:00) de ese día local.
        private static (DateTime fromUtc, DateTime toUtc) GetUtcRangeForLocalDate(DateTime fechaLocal)
        {
            var tz = GetGtTz();
            var fromLocal = fechaLocal.Date;
            var toLocal = fromLocal.AddDays(1);

            var fromUtc = TimeZoneInfo.ConvertTimeToUtc(fromLocal, tz);
            var toUtc = TimeZoneInfo.ConvertTimeToUtc(toLocal, tz);
            return (fromUtc, toUtc);
        }

        // ===== Queries =====

        /// GET /api/aperturacaja/por-fecha?fecha=YYYY-MM-DD
        /// Devuelve la apertura del día (o null). No filtra por usuario para permitir guard global;
        /// si quieres por usuario, añade && a.UsuarioId == GetUserId() en el Where.
        [HttpGet("por-fecha")]
        public async Task<ActionResult<object>> GetPorFecha([FromQuery] DateTime? fecha)
        {
            if (fecha is null) return BadRequest("fecha requerida (yyyy-MM-dd)");

            var (fromUtc, toUtc) = GetUtcRangeForLocalDate(fecha.Value);

            var ap = await _db.AperturasCaja
                .AsNoTracking()
                .Where(a => a.FechaAperturaUtc >= fromUtc && a.FechaAperturaUtc < toUtc)
                .OrderByDescending(a => a.Id)
                .Select(a => new
                {
                    a.Id,
                    a.UsuarioId,
                    a.CajeroNombre,
                    a.FechaAperturaUtc,
                    montoApertura = a.MontoApertura,
                    a.Notas
                })
                .FirstOrDefaultAsync();

            return Ok(ap); // 200 con objeto o con null
        }

        /// GET /api/aperturacaja/existe-hoy  → true/false
        [HttpGet("existe-hoy")]
        public async Task<ActionResult<bool>> ExisteHoy()
        {
            var hoyLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetGtTz()).Date;
            var (fromUtc, toUtc) = GetUtcRangeForLocalDate(hoyLocal);

            var ok = await _db.AperturasCaja
                .AsNoTracking()
                .AnyAsync(a => a.FechaAperturaUtc >= fromUtc && a.FechaAperturaUtc < toUtc
                               && a.MontoApertura > 0);

            return Ok(ok);
        }

        /// GET /api/aperturacaja?fecha=YYYY-MM-DD  (similar al tuyo original; devuelve entidad)
        [HttpGet]
        public async Task<ActionResult<AperturaCaja?>> GetByDate([FromQuery] DateTime? fecha)
        {
            var date = (fecha ?? DateTime.UtcNow);
            var (fromUtc, toUtc) = GetUtcRangeForLocalDate(
                TimeZoneInfo.ConvertTimeFromUtc(date, GetGtTz()).Date);

            var apertura = await _db.AperturasCaja
                .AsNoTracking()
                .Where(a => a.FechaAperturaUtc >= fromUtc && a.FechaAperturaUtc < toUtc)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();

            return Ok(apertura); // puede ser null
        }

        // ===== Upsert (crear/actualizar) =====

        /// PUT /api/aperturacaja  (upsert por día)
        [HttpPut]
        public async Task<ActionResult<AperturaCaja>> Upsert([FromBody] AperturaCajaCreateDto dto)
        {
            if (dto is null) return BadRequest("Body requerido.");
            if (dto.MontoApertura < 0) return BadRequest("El monto debe ser >= 0.");

            var userId = GetUserId();

            // Día local recibido (si viene FechaAperturaUtc en local/UTC, normalizamos a local-date)
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(
                dto.FechaAperturaUtc.Kind == DateTimeKind.Utc
                    ? dto.FechaAperturaUtc
                    : dto.FechaAperturaUtc.ToUniversalTime(),
                GetGtTz()
            ).Date;

            var (fromUtc, toUtc) = GetUtcRangeForLocalDate(localDate);

            // ¿ya existe para ese día?
            var existente = await _db.AperturasCaja
                .FirstOrDefaultAsync(a => a.FechaAperturaUtc >= fromUtc && a.FechaAperturaUtc < toUtc);

            if (existente is not null)
            {
                existente.MontoApertura = dto.MontoApertura;
                existente.Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim();
                existente.CajeroNombre = string.IsNullOrWhiteSpace(dto.CajeroNombre) ? existente.CajeroNombre : dto.CajeroNombre;
                await _db.SaveChangesAsync();
                return Ok(existente);
            }

            var entidad = new AperturaCaja
            {
                UsuarioId = userId,
                CajeroNombre = string.IsNullOrWhiteSpace(dto.CajeroNombre) ? "Cajero" : dto.CajeroNombre.Trim(),
                // Guarda en UTC (ahora). Alternativamente, guarda el inicio del día local en UTC:
                // FechaAperturaUtc = TimeZoneInfo.ConvertTimeToUtc(localDate, GetGtTz()),
                FechaAperturaUtc = DateTime.UtcNow,
                MontoApertura = dto.MontoApertura,
                Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim()
            };

            _db.AperturasCaja.Add(entidad);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByDate), new { fecha = localDate }, entidad);
        }

        /// POST /api/aperturacaja  (alias de upsert)
        [HttpPost]
        public Task<ActionResult<AperturaCaja>> Create([FromBody] AperturaCajaCreateDto dto)
            => Upsert(dto);

        /// GET /api/aperturacaja/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AperturaCaja>> GetById(int id)
        {
            var a = await _db.AperturasCaja.FindAsync(id);
            return a is null ? NotFound() : Ok(a);
        }
    }
}
