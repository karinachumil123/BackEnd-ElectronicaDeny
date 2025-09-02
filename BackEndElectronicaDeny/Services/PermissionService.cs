using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_ElectronicaDeny.Services
{
    public class PermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Permiso>> GetPermisosForRolAsync(int rolId)
        {
            return await _context.RolPermisos
                .Where(rp => rp.RolId == rolId)
                .Include(rp => rp.Permiso)
                .Select(rp => rp.Permiso)
                .ToListAsync();
        }

        public async Task<bool> AddPermisoToRolAsync(int rolId, int permisoId)
        {
            // Verificar si ya existe la relación
            var existente = await _context.RolPermisos
                .AnyAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);

            if (existente)
                return false;

            _context.RolPermisos.Add(new RolPermiso
            {
                RolId = rolId,
                PermisoId = permisoId
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermisoFromRolAsync(int rolId, int permisoId)
        {
            var rolPermiso = await _context.RolPermisos
                .FirstOrDefaultAsync(rp => rp.RolId == rolId && rp.PermisoId == permisoId);

            if (rolPermiso == null)
                return false;

            _context.RolPermisos.Remove(rolPermiso);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                return false;

            var tienePermiso = await _context.RolPermisos
                .Include(rp => rp.Permiso)
                .AnyAsync(rp => rp.RolId == usuario.RolId && rp.Permiso.Nombre == permissionName);

            return tienePermiso;
        }
    }
}
