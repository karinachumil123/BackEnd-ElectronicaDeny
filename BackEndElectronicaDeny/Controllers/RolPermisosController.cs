using BackEnd_ElectronicaDeny.Data;
using Microsoft.AspNetCore.Mvc;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_ElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolPermisosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolPermisosController(AppDbContext context)
        {
            _context = context;
        }

        // Asignar permisos a un rol
        [HttpPost("{rolId}/asignar-permisos")]
        public async Task<IActionResult> AsignarPermisosARol(int rolId, [FromBody] List<int> permisoIds)
        {
            // Verificar si el rol existe
            var rol = await _context.Roles.FindAsync(rolId);
            if (rol == null)
            {
                return NotFound("El rol no existe.");
            }

            // Buscar los permisos en la base de datos
            var permisos = await _context.Permisos
                .Where(p => permisoIds.Contains(p.Id))
                .ToListAsync();

            if (!permisos.Any())
            {
                return NotFound("No se encontraron permisos.");
            }

            // Asignar los permisos al rol (solo si no están ya asignados)
            foreach (var permiso in permisos)
            {
                var rolPermisoExistente = await _context.RolPermisos
                    .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PermisoId == permiso.Id);

                if (rolPermisoExistente == null)
                {
                    _context.RolPermisos.Add(new RolPermiso { RolId = rolId, PermisoId = permiso.Id });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Permisos asignados correctamente" });
        }

        [HttpGet("{rolId:int}/permisos")]
        public async Task<IActionResult> ObtenerPermisosDeRol(int rolId)
        {
            var permisos = await _context.RolPermisos
                .Where(rp => rp.RolId == rolId)
                .Select(rp => new { Id = rp.PermisoId, Nombre = rp.Permiso!.Nombre })
                .ToListAsync();

            return Ok(permisos);
        }

        [HttpPut("{rolId}/actualizar-permisos")]
        public async Task<IActionResult> ActualizarPermisosDeRol(int rolId, [FromBody] List<int> permisoIds)
        {
            try
            {
                var rol = await _context.Roles.FindAsync(rolId);
                if (rol == null) return NotFound("El rol no existe.");

                var actuales = _context.RolPermisos.Where(rp => rp.RolId == rolId);
                _context.RolPermisos.RemoveRange(actuales);

                var nuevos = permisoIds.Select(id => new RolPermiso { RolId = rolId, PermisoId = id });
                await _context.RolPermisos.AddRangeAsync(nuevos);

                await _context.SaveChangesAsync();
                return Ok(new { message = "Permisos actualizados correctamente." });
            }
            catch (Exception ex)
            {
                // Log detallado
                Console.WriteLine($"Error al actualizar permisos: {ex.Message}");
                Console.WriteLine(ex.InnerException?.Message);
                return StatusCode(500, $"Error al guardar cambios: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

    }
}
