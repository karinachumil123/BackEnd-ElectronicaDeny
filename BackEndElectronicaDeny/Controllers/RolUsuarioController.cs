using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_ElectronicaDeny.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolUsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolUsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // Obtener todos los roles de usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolUsuario>>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        // Obtener un rol por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<RolUsuario>> GetRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();
            return rol;
        }

        // Crear un nuevo rol
        [HttpPost]
        public async Task<ActionResult<RolUsuario>> PostRol(RolUsuario rol)
        {
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRol), new { id = rol.Id }, rol);
        }

        // Actualizar un rol
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRol(int id, RolUsuario rol)
        {
            if (id != rol.Id) return BadRequest();
            _context.Entry(rol).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Eliminar un rol
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();
            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return NoContent();
        }



        // 6. Asignar usuarios a un rol (POST)
        [HttpPost("{rolId}/usuarios")]
        public async Task<ActionResult> AssignUsersToRole(int rolId, [FromBody] List<int> userIds)
        {
            var rol = await _context.Roles.Include(r => r.Usuarios).FirstOrDefaultAsync(r => r.Id == rolId);

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            foreach (var userId in userIds)
            {
                var user = await _context.Usuarios.FindAsync(userId);
                if (user != null)
                {
                    user.RolId = rolId;  // Asigna el rol al usuario
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 7. Asignar permisos a un rol (POST)
        [HttpPost("{rolId}/permisos")]
        public async Task<ActionResult> AssignPermissionsToRole(int rolId, [FromBody] List<int> permisoIds)
        {
            var rol = await _context.Roles.Include(r => r.Permisos).FirstOrDefaultAsync(r => r.Id == rolId);

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            foreach (var permisoId in permisoIds)
            {
                var permiso = await _context.Permisos.FindAsync(permisoId);
                if (permiso != null)
                {
                    rol.Permisos.Add(permiso);  // Agregar permiso al rol
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 8. Obtener usuarios asignados a un rol (GET)
        [HttpGet("{rolId}/usuarios")]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsersByRole(int rolId)
        {
            var rol = await _context.Roles.Include(r => r.Usuarios)
                                          .FirstOrDefaultAsync(r => r.Id == rolId);

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            // Devolver los usuarios asignados a este rol
            return Ok(rol.Usuarios);
        }

        // 9. Obtener permisos asignados a un rol (GET)
        [HttpGet("{rolId}/permisos")]
        public async Task<ActionResult<IEnumerable<Permiso>>> GetPermissionsByRole(int rolId)
        {
            var rol = await _context.Roles.Include(r => r.Permisos)
                                          .FirstOrDefaultAsync(r => r.Id == rolId);

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            // Devolver los permisos asignados a este rol
            return Ok(rol.Permisos);
        }

    }
}
