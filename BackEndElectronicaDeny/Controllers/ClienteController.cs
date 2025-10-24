using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.DTOs;
using BackEndElectronicaDeny.Models;
using Microsoft.Extensions.Logging;

namespace BackEnd_ElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(AppDbContext context, ILogger<ClienteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static string EstadoNombre(int estado) =>
            estado == 1 ? "Activo" :
            estado == 2 ? "Inactivo" :
            $"Desconocido({estado})";

        // GET: api/cliente
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            try
            {
                var list = await _context.Clientes
                    .AsNoTracking()
                    .OrderByDescending(c => c.FechaRegistro)
                    .ToListAsync();

                var dto = list.Select(c => new ClienteDto
                {
                    Id = c.Id,
                    NombreCompleto = c.NombreCompleto,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    NIT = c.NIT,
                    Direccion = c.Direccion,
                    Municipio = c.Municipio,
                    Departamento = c.Departamento,
                    Estado = c.Estado,
                    FechaRegistroTexto = c.FechaRegistro.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                return StatusCode(500, "Error interno al obtener clientes.");
            }
        }

        // GET: api/cliente/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            try
            {
                var c = await _context.Clientes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (c == null)
                    return NotFound($"No se encontró el cliente con ID {id}");

                var dto = new ClienteDto
                {
                    Id = c.Id,
                    NombreCompleto = c.NombreCompleto,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    NIT = c.NIT,
                    Direccion = c.Direccion,
                    Municipio = c.Municipio,
                    Departamento = c.Departamento,
                    Estado = c.Estado,
                    FechaRegistroTexto = c.FechaRegistro.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente {Id}", id);
                return StatusCode(500, "Error interno al obtener el cliente.");
            }
        }

        // (Opcional) GET: api/cliente/existe-telefono?tel=xxxx
        [HttpGet("existe-telefono")]
        public async Task<ActionResult<object>> ExisteTelefono([FromQuery] string tel)
        {
            var telefono = (tel ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(telefono))
                return BadRequest(new { message = "Debe enviar el parámetro 'tel'." });

            var existe = await _context.Clientes.AsNoTracking()
                .AnyAsync(c => c.Telefono == telefono);

            return Ok(new { existe });
        }

        // POST: api/cliente
        // Crea cliente. Estado se fuerza a 1 (Activo).
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente([FromBody] ClienteDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload vacío.");

                if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
                    return BadRequest("El nombre completo es obligatorio.");
                if (string.IsNullOrWhiteSpace(dto.Telefono))
                    return BadRequest("El teléfono es obligatorio.");

                var telefono = dto.Telefono.Trim();

                // Validación explícita de duplicado
                var telDuplicado = await _context.Clientes.AsNoTracking()
                    .AnyAsync(c => c.Telefono == telefono);
                if (telDuplicado)
                    return Conflict(new { message = $"Ya existe un cliente registrado con el teléfono {telefono}." });

                var entity = new Clientes
                {
                    NombreCompleto = dto.NombreCompleto.Trim(),
                    Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim(),
                    Telefono = telefono,
                    NIT = string.IsNullOrWhiteSpace(dto.NIT) ? null : dto.NIT.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim(),
                    Municipio = string.IsNullOrWhiteSpace(dto.Municipio) ? null : dto.Municipio.Trim(),
                    Departamento = string.IsNullOrWhiteSpace(dto.Departamento) ? null : dto.Departamento.Trim(),
                    Estado = 1, // SIEMPRE ACTIVO AL CREAR
                    FechaRegistro = DateTime.UtcNow
                };

                try
                {
                    _context.Clientes.Add(entity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Respaldo por restricción única desde DB
                    if (EsUniqueViolation(ex))
                        return Conflict(new { message = $"Ya existe un cliente registrado con el teléfono {telefono}." });
                    throw;
                }

                dto.Id = entity.Id;
                dto.Estado = entity.Estado;
                dto.FechaRegistroTexto = entity.FechaRegistro.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                return CreatedAtAction(nameof(GetCliente), new { id = entity.Id }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cliente");
                return StatusCode(500, "Error interno al crear cliente.");
            }
        }

        // PUT: api/cliente/5
        // Actualiza datos (Estado no se edita en tu UI; si llega 1/2 se respeta)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCliente(int id, [FromBody] ClienteDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload vacío.");
                if (id != dto.Id) return BadRequest("El ID no coincide.");

                var entity = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound($"No se encontró el cliente con ID {id}");

                if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
                    return BadRequest("El nombre completo es obligatorio.");
                if (string.IsNullOrWhiteSpace(dto.Telefono))
                    return BadRequest("El teléfono es obligatorio.");

                var telefono = dto.Telefono.Trim();

                // El teléfono no puede estar tomado por otro cliente
                var telTomadoPorOtro = await _context.Clientes.AsNoTracking()
                    .AnyAsync(c => c.Telefono == telefono && c.Id != id);
                if (telTomadoPorOtro)
                    return Conflict(new { message = $"El teléfono {telefono} ya está asignado a otro cliente." });

                entity.NombreCompleto = dto.NombreCompleto.Trim();
                entity.Correo = string.IsNullOrWhiteSpace(dto.Correo) ? null : dto.Correo.Trim();
                entity.Telefono = telefono;
                entity.NIT = string.IsNullOrWhiteSpace(dto.NIT) ? null : dto.NIT.Trim();
                entity.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
                entity.Municipio = string.IsNullOrWhiteSpace(dto.Municipio) ? null : dto.Municipio.Trim();
                entity.Departamento = string.IsNullOrWhiteSpace(dto.Departamento) ? null : dto.Departamento.Trim();

                if (dto.Estado == 1 || dto.Estado == 2)
                    entity.Estado = dto.Estado;

                try
                {
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                catch (DbUpdateException ex)
                {
                    if (EsUniqueViolation(ex))
                        return Conflict(new { message = $"El teléfono {telefono} ya está asignado a otro cliente." });
                    throw;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrencia al actualizar cliente {Id}", id);
                return StatusCode(409, "Error de concurrencia al actualizar el cliente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente {Id}", id);
                return StatusCode(500, "Error interno al actualizar cliente.");
            }
        }

        // PUT: api/cliente/eliminar-logico/5  => Estado = 2
        [HttpPut("eliminar-logico/{id:int}")]
        public async Task<IActionResult> EliminarLogicoCliente(int id)
        {
            try
            {
                var entity = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound($"No se encontró el cliente con ID {id}");

                entity.Estado = 2; // Inactivo
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inactivar cliente {Id}", id);
                return StatusCode(500, "Error interno al inactivar cliente.");
            }
        }

        // DELETE físico (opcional)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                var entity = await _context.Clientes.FindAsync(id);
                if (entity == null) return NotFound($"No se encontró el cliente con ID {id}");

                _context.Clientes.Remove(entity);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente {Id}", id);
                return StatusCode(500, "Error interno al eliminar cliente.");
            }
        }

        // ==== Helpers ====
        private static bool EsUniqueViolation(DbUpdateException ex)
        {
            // PostgreSQL: SqlState 23505
            if (ex.InnerException?.Message?.Contains("23505") == true) return true;

            // SQL Server: 2601 / 2627
            if (ex.InnerException?.Message?.Contains("2601") == true) return true;
            if (ex.InnerException?.Message?.Contains("2627") == true) return true;

            // Fallback por texto
            var msg = ex.InnerException?.Message?.ToLower() ?? "";
            return msg.Contains("unique") || msg.Contains("índice único") || msg.Contains("unique constraint");
        }
    }
}
