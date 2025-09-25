using BackEnd_ElectronicaDeny.Data;     // <-- ajusta si tu AppDbContext está en otro namespace
using BackEndElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/proveedores?incluirInactivos=false
        // Por defecto SOLO devuelve Activos (EstadoId == 1)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedores([FromQuery] bool incluirInactivos = false)
        {
            IQueryable<Proveedor> q = _context.Proveedores.AsNoTracking();

            if (!incluirInactivos)
                q = q.Where(p => p.EstadoId == 1); // solo activos

            var list = await q.ToListAsync();
            return Ok(list);
        }

        // GET: api/proveedores/activos
        [HttpGet("activos")]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedoresActivos()
        {
            var activos = await _context.Proveedores
                .Where(p => p.EstadoId == 1)
                .AsNoTracking()
                .ToListAsync();

            return Ok(activos);
        }

        // GET: api/proveedores/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Proveedor>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (proveedor == null) return NotFound();
            return proveedor;
        }

        // GET: api/proveedores/{proveedorId}/productos
        // Devuelve SOLO productos ACTIVOS del proveedor indicado
        [HttpGet("{proveedorId:int}/productos")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductosPorProveedor(int proveedorId)
        {
            var productos = await _context.Productos
                .Where(p => p.ProveedorId == proveedorId && p.EstadoId == 1) // solo activos
                .AsNoTracking()
                .Select(p => new { p.Id, p.Nombre, p.ProveedorId })
                .ToListAsync();

            return Ok(productos);
        }

        // POST: api/proveedores
        [HttpPost]
        public async Task<ActionResult<Proveedor>> AgregarProveedor([FromBody] Proveedor proveedor)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Normaliza estado (1 Activo, 2 Inactivo)
            if (proveedor.EstadoId != 1 && proveedor.EstadoId != 2)
                proveedor.EstadoId = 1;

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.Id }, proveedor);
        }

        // PUT: api/proveedores/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProveedor(int id, [FromBody] Proveedor proveedor)
        {
            if (id != proveedor.Id)
                return BadRequest("El id del path no coincide con el cuerpo de la solicitud.");

            var proveedorDb = await _context.Proveedores.FindAsync(id);
            if (proveedorDb == null)
                return NotFound();

            // Validar estado 1/2
            if (proveedor.EstadoId != 1 && proveedor.EstadoId != 2)
                return BadRequest("EstadoId inválido. Solo se permite 1 (Activo) o 2 (Inactivo).");

            // Actualiza campos
            proveedorDb.Nombre = proveedor.Nombre;
            proveedorDb.NombreContacto = proveedor.NombreContacto;
            proveedorDb.Telefono = proveedor.Telefono;
            proveedorDb.TelefonoContacto = proveedor.TelefonoContacto;
            proveedorDb.Correo = proveedor.Correo;
            proveedorDb.Direccion = proveedor.Direccion;
            proveedorDb.Descripcion = proveedor.Descripcion;
            proveedorDb.EstadoId = proveedor.EstadoId; // 1 o 2

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/proveedores/5  (soft delete -> Inactivo)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
                return NotFound();

            // ¿Está en uso? (ajusta si tienes más relaciones)
            bool enUso =
                await _context.Productos.AnyAsync(p => p.ProveedorId == id) ||
                await _context.Pedidos.AnyAsync(c => c.ProveedorId == id); // DbSet singular: Pedido

            if (enUso)
                return Conflict("No se puede inactivar/eliminar el proveedor porque está en uso.");

            // Soft delete: inactivar
            if (proveedor.EstadoId != 2)
            {
                proveedor.EstadoId = 2;
                _context.Entry(proveedor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // GET: api/proveedores/esta-en-uso/{id}
        [HttpGet("esta-en-uso/{id:int}")]
        public async Task<ActionResult<bool>> EstaEnUso(int id)
        {
            bool enUso =
                await _context.Productos.AnyAsync(p => p.ProveedorId == id) ||
                await _context.Pedidos.AnyAsync(c => c.ProveedorId == id); // DbSet singular: Pedido

            return Ok(enUso);
        }

        private bool ProveedorExists(int id) =>
            _context.Proveedores.Any(e => e.Id == id);
    }
}
