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

        // Devuelve todos los permisos (solo para poblar la DB)
        public async Task<List<Permiso>> GetAllPermisosAsync()
        {
            return await _context.Permisos.ToListAsync();
        }

        // Añade un permiso simple (solo para crear registros)
        public async Task AddPermisoAsync(string nombre)
        {
            if (!await _context.Permisos.AnyAsync(p => p.Nombre == nombre))
            {
                _context.Permisos.Add(new Permiso { Nombre = nombre });
                await _context.SaveChangesAsync();
            }
        }
    }
}
