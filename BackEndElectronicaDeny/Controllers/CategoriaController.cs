using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
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
        // GET: api/categorias
        // incluirEliminados (legacy nombre):
        //   - true  => devuelve TODAS (estadoId 1 y 2)
        //   - false => SOLO ACTIVAS (estadoId 1)
        // =========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias([FromQuery] bool incluirEliminados = false)
        {
            IQueryable<Categoria> q = _context.Categorias.AsNoTracking();

            if (!incluirEliminados)
                q = q.Where(c => c.EstadoId == 1); // Solo activas

            var list = await q.ToListAsync();
            return Ok(list);
        }

        // (Opcional) Solo activas
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasActivas()
        {
            var activas = await _context.Categorias
                .Where(c => c.EstadoId == 1)
                .AsNoTracking()
                .ToListAsync();

            return Ok(activas);
        }

        // GET: api/categorias/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return categoria;
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria([FromBody] Categoria categoria)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Asegurar valores válidos (1 o 2)
            if (categoria.EstadoId != 1 && categoria.EstadoId != 2)
                categoria.EstadoId = 1; // default Activo

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, categoria);
        }

        // PUT: api/categorias/5
        // Permite inactivar (2) aunque esté en uso. 
        // La lógica para impedir nuevas asociaciones con inactivas va en otros controladores.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCategoria(int id, [FromBody] Categoria categoria)
        {
            if (id != categoria.Id) return BadRequest();

            var categoriaDb = await _context.Categorias.FindAsync(id);
            if (categoriaDb == null) return NotFound();

            // Validar estado válido
            if (categoria.EstadoId != 1 && categoria.EstadoId != 2)
                return BadRequest("EstadoId inválido. Solo se permite 1 (Activo) o 2 (Inactivo).");

            categoriaDb.CategoriaNombre = categoria.CategoriaNombre;
            categoriaDb.Descripcion = categoria.Descripcion;
            categoriaDb.EstadoId = categoria.EstadoId; // 1 o 2

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/categorias/5
        // Soft delete => marcar como Inactivo (2), incluso si está en uso.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            // Soft delete: inactivar
            if (categoria.EstadoId != 2)
            {
                categoria.EstadoId = 2;
                _context.Entry(categoria).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        // GET: api/categorias/esta-en-uso/{id}
        [HttpGet("esta-en-uso/{id:int}")]
        public async Task<ActionResult<bool>> EstaEnUso(int id)
        {
            // Ajusta si tu categoría se usa en más tablas
            bool enUso = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            return Ok(enUso);
        }
    }
}


