using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // Helper para obtener Id de 'Eliminado'
        private async Task<int> GetEstadoEliminadoIdAsync()
        {
            return await _context.Estados
                .Where(e => e.Nombre == "Eliminado")
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
        }

        // GET: api/proveedores
        // Por defecto NO trae Eliminados. Pásale ?incluirEliminados=true solo en pantallas administrativas.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedores([FromQuery] bool incluirEliminados = false)
        {
            var idEliminado = await GetEstadoEliminadoIdAsync();

            var q = _context.Proveedores
                .Include(p => p.Estado)
                .AsNoTracking();

            if (!incluirEliminados && idEliminado != 0)
                q = q.Where(p => p.EstadoId != idEliminado);

            return await q.ToListAsync();
        }

        // GET: api/proveedores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Proveedor>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.Estado)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proveedor == null)
                return NotFound();

            return proveedor;
        }

        // POST: api/proveedores
        [HttpPost]
        public async Task<ActionResult<Proveedor>> AgregarProveedor(Proveedor proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedor);
        }

        // PUT: api/proveedores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProveedor(int id, Proveedor proveedor)
        {
            if (id != proveedor.Id)
                return BadRequest();

            var proveedorDb = await _context.Proveedores.FindAsync(id);
            if (proveedorDb == null)
                return NotFound();

            // Actualiza campos según tu modelo
            proveedorDb.Nombre = proveedor.Nombre;
            proveedorDb.NombreContacto = proveedor.NombreContacto;
            proveedorDb.Telefono = proveedor.Telefono;
            proveedorDb.TelefonoContacto = proveedor.TelefonoContacto;
            proveedorDb.Correo = proveedor.Correo;
            proveedorDb.Direccion = proveedor.Direccion;
            proveedorDb.Descripcion = proveedor.Descripcion;
            proveedorDb.EstadoId = proveedor.EstadoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/proveedores/5 (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound();

            // Verificar si está en uso (ajusta relaciones reales)
            bool enUso =
                await _context.Productos.AnyAsync(p => p.ProveedorId == id) ||
                await _context.Pedido.AnyAsync(c => c.ProveedorId == id);

            if (enUso)
                return Conflict("No se puede eliminar el proveedor porque está en uso.");

            var idEliminado = await GetEstadoEliminadoIdAsync();
            if (idEliminado == 0)
                return StatusCode(500, "No está configurado el estado 'Eliminado' en la tabla de Estados.");

            if (proveedor.EstadoId != idEliminado)
            {
                proveedor.EstadoId = idEliminado;      // soft delete
                _context.Entry(proveedor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.Id == id);
        }
    }
}


