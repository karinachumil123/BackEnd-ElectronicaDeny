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


        // Obtener los permisos asignados a un rol
        [HttpGet("{rolId}/permisos")]
        public async Task<IActionResult> ObtenerPermisosDeRol(int rolId)
        {
            var permisos = await _context.RolPermisos
                .Where(rp => rp.RolId == rolId)
                .Select(rp => new { rp.Permiso.Id, rp.Permiso.Nombre }) // Solo id y nombre del permiso
                .ToListAsync();

            if (!permisos.Any())
            {
                return NotFound("Este rol no tiene permisos asignados.");
            }

            return Ok(permisos);
        }

        [HttpPut("{rolId}/actualizar-permisos")]
        public async Task<IActionResult> ActualizarPermisosDeRol(int rolId, [FromBody] List<int> permisoIds)
        {
            var rol = await _context.Roles.FindAsync(rolId);
            if (rol == null)
            {
                return NotFound("El rol no existe.");
            }

            // 1️⃣ Eliminar permisos existentes
            var permisosActuales = _context.RolPermisos.Where(rp => rp.RolId == rolId);
            _context.RolPermisos.RemoveRange(permisosActuales);

            // 2️⃣ Agregar solo los permisos enviados
            var nuevosPermisos = permisoIds.Select(id => new RolPermiso { RolId = rolId, PermisoId = id });
            await _context.RolPermisos.AddRangeAsync(nuevosPermisos);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Permisos actualizados correctamente." });
        }
    }
}
