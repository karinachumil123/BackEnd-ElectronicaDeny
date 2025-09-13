using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using BackEndElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // Helpers: Id por Nombre
        // =========================
        private Task<int> GetEstadoIdAsync(string nombre)
        {
            return _context.Estados
                .Where(e => e.Nombre == nombre)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
        }

        private Task<int> GetEstadoEliminadoIdAsync() => GetEstadoIdAsync("Eliminado");
        private Task<int> GetEstadoInactivoIdAsync() => GetEstadoIdAsync("Inactivo");
        private Task<int> GetEstadoActivoIdAsync() => GetEstadoIdAsync("Activo");

        // =========================
        // GET: api/categorias
        // Por defecto NO devuelve Eliminadas,
        // para administración: ?incluirEliminados=true
        // =========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias([FromQuery] bool incluirEliminados = false)
        {
            var idEliminado = await GetEstadoEliminadoIdAsync();

            var q = _context.Categorias
                .Include(c => c.Estado)
                .AsNoTracking();

            if (!incluirEliminados && idEliminado != 0)
                q = q.Where(c => c.EstadoId != idEliminado);

            return await q.ToListAsync();
        }

        // =========================
        // (Opcional) Solo activas para otros módulos
        // GET: api/categorias/activas
        // =========================
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasActivas()
        {
            var idActivo = await GetEstadoActivoIdAsync();
            if (idActivo == 0) return StatusCode(500, "No está configurado el estado 'Activo'.");

            var listado = await _context.Categorias
                .Include(c => c.Estado)
                .Where(c => c.EstadoId == idActivo)
                .AsNoTracking()
                .ToListAsync();

            return Ok(listado);
        }

        // =========================
        // GET: api/categorias/5
        // =========================
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Estado)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
                return NotFound();

            return categoria;
        }

        // =========================
        // POST: api/categorias
        // =========================
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
        }

        // =========================
        // PUT: api/categorias/5
        // Bloquea cambio a Inactivo/Eliminado si está en uso
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            if (id != categoria.Id)
                return BadRequest();

            var categoriaDb = await _context.Categorias.FindAsync(id);
            if (categoriaDb == null)
                return NotFound();

            // Verificar si intenta pasar a Inactivo/Eliminado
            var idInactivo = await GetEstadoInactivoIdAsync();
            var idEliminado = await GetEstadoEliminadoIdAsync();
            bool vaInactivarOEliminar = (categoria.EstadoId == idInactivo && idInactivo != 0)
                                        || (categoria.EstadoId == idEliminado && idEliminado != 0);

            if (vaInactivarOEliminar)
            {
                // ¿Está en uso? (ej. referenciado por Productos)
                bool enUso = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
                if (enUso)
                {
                    // 409 Conflict: negocio no lo permite
                    return Conflict("No puedes cambiar el estado a 'Inactivo' o 'Eliminado' porque la categoría está en uso en otros módulos.");
                }
            }

            // Actualiza campos permitidos
            categoriaDb.CategoriaNombre = categoria.CategoriaNombre;
            categoriaDb.Descripcion = categoria.Descripcion;
            categoriaDb.EstadoId = categoria.EstadoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================
        // DELETE: api/categorias/5 (Soft delete)
        // Si está en uso -> 409
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            bool enUso = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            if (enUso)
                return Conflict("No se puede eliminar la categoría porque está en uso.");

            var idEliminado = await GetEstadoEliminadoIdAsync();
            if (idEliminado == 0)
                return StatusCode(500, "No está configurado el estado 'Eliminado' en la tabla de Estados.");

            if (categoria.EstadoId != idEliminado)
            {
                categoria.EstadoId = idEliminado;
                _context.Entry(categoria).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // =========================
        // GET: api/categorias/estados
        // =========================
        [HttpGet("estados")]
        public async Task<ActionResult<IEnumerable<Estados>>> GetEstados()
        {
            var estados = await _context.Estados.AsNoTracking().ToListAsync();
            return Ok(estados);
        }

        // =========================
        // GET: api/categorias/esta-en-uso/{id}
        // Endpoint para el front: true/false
        // =========================
        [HttpGet("esta-en-uso/{id:int}")]
        public async Task<ActionResult<bool>> EstaEnUso(int id)
        {
            // Amplía estas comprobaciones si la categoría se usa en otros módulos/tablas
            bool enUso = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            return Ok(enUso);
        }
    }
}



