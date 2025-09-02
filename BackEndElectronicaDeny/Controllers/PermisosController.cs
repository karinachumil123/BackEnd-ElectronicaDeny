using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermisosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermisosController(AppDbContext context)
        {
            _context = context;
        }

        // Crear un nuevo permiso
        [HttpPost]
        public async Task<IActionResult> CrearPermiso([FromBody] Permiso permiso)
        {
            if (permiso == null)
            {
                return BadRequest("El permiso es inválido.");
            }

            // Agregar el permiso a la base de datos
            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();

            // Retornar solo el id y nombre del permiso creado
            var permisoCreado = new { id = permiso.Id, nombre = permiso.Nombre };

            return CreatedAtAction(nameof(ObtenerPermiso), new { id = permiso.Id }, permisoCreado);
        }

        // Obtener todos los permisos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permiso>>> ObtenerPermisos()
        {
            var permisos = await _context.Permisos
                .Select(p => new { p.Id, p.Nombre }) // Solo id y nombre del permiso
                .ToListAsync();

            return Ok(permisos);
        }

        // Obtener un permiso por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Permiso>> ObtenerPermiso(int id)
        {
            var permiso = await _context.Permisos
                .Where(p => p.Id == id)
                .Select(p => new { p.Id, p.Nombre }) // Solo id y nombre
                .FirstOrDefaultAsync();

            if (permiso == null)
            {
                return NotFound();
            }

            return Ok(permiso);
        }

        // Editar un permiso
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarPermiso(int id, [FromBody] Permiso permiso)
        {
            if (id != permiso.Id)
            {
                return BadRequest("El ID del permiso no coincide.");
            }

            var permisoExistente = await _context.Permisos.FindAsync(id);
            if (permisoExistente == null)
            {
                return NotFound("El permiso no existe.");
            }

            // Actualizar el nombre del permiso
            permisoExistente.Nombre = permiso.Nombre;

            // Guardar los cambios
            await _context.SaveChangesAsync();

            return NoContent(); // Retorna 204 si la actualización fue exitosa
        }

        // Eliminar un permiso
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPermiso(int id)
        {
            var permiso = await _context.Permisos.FindAsync(id);
            if (permiso == null)
            {
                return NotFound("El permiso no existe.");
            }

            _context.Permisos.Remove(permiso);
            await _context.SaveChangesAsync();

            return NoContent(); // Retorna 204 si la eliminación fue exitosa
        }
        [HttpPut("{rolId}/permisos")]
        public async Task<IActionResult> ActualizarPermisosRol(int rolId, [FromBody] List<int> nuevosPermisosIds)
        {
            // Validar la entrada
            if (nuevosPermisosIds == null || !nuevosPermisosIds.Any())
            {
                return BadRequest("La lista de permisos no puede ser nula o vacía.");
            }

            // Usar una transacción explícita para asegurar consistencia
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verificar que el rol exista
                var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Id == rolId);
                if (rol == null)
                {
                    Console.WriteLine($"Error: El rol con ID {rolId} no fue encontrado.");
                    return NotFound($"El rol con ID {rolId} no fue encontrado.");
                }

                // Verificar que todos los permisos a asignar existan
                var permisosExistentes = await _context.Permisos
                    .Where(p => nuevosPermisosIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var permisosNoExistentes = nuevosPermisosIds.Except(permisosExistentes).ToList();
                if (permisosNoExistentes.Any())
                {
                    Console.WriteLine($"Error: Los siguientes permisos no existen: {string.Join(", ", permisosNoExistentes)}");
                    return BadRequest($"Los siguientes permisos no existen: {string.Join(", ", permisosNoExistentes)}");
                }

                // PASO 1: Eliminar TODOS los permisos actuales del rol
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM \"RolPermisos\" WHERE \"RolId\" = {0}", rolId);

                // PASO 2: Insertar TODOS los nuevos permisos
                if (nuevosPermisosIds.Any())
                {
                    var nuevosRolPermisos = nuevosPermisosIds.Select(permisoId => new RolPermiso
                    {
                        RolId = rolId,
                        PermisoId = permisoId
                    }).ToList();

                    await _context.RolPermisos.AddRangeAsync(nuevosRolPermisos);
                    await _context.SaveChangesAsync();
                }

                // Confirmar la transacción
                await transaction.CommitAsync();

                // Verificar que los cambios se aplicaron correctamente
                var permisosAsignados = await _context.RolPermisos
                    .Where(rp => rp.RolId == rolId)
                    .Select(rp => rp.PermisoId)
                    .ToListAsync();

                return Ok(new
                {
                    mensaje = "Permisos actualizados correctamente",
                    rol = new { id = rol.Id, nombre = rol.Nombre },
                    permisosAsignados = permisosAsignados
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error al actualizar permisos: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Error interno: {ex.InnerException.Message}");
                }

                return BadRequest($"Error al actualizar los permisos: {ex.Message}");
            }
        }

    }
}
