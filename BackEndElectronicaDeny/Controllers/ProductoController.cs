using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.DTOs;
using BackEndElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(AppDbContext context, ILogger<ProductoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Helper: Id del estado 'Eliminado'
        private async Task<int> GetEstadoEliminadoIdAsync()
        {
            return await _context.Estados
                .Where(e => e.Nombre == "Eliminado")
                .Select(e => e.Id)
                .FirstOrDefaultAsync();
        }

        // GET: api/Productos
        // Por defecto NO devuelve productos en 'Eliminado'.
        // Para listados administrativos: api/Producto?incluirEliminados=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProductos([FromQuery] bool incluirEliminados = false)
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de productos");

                var idEliminado = await GetEstadoEliminadoIdAsync();

                var q = _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .Include(p => p.Estado)
                    .AsNoTracking();

                if (!incluirEliminados && idEliminado != 0)
                    q = q.Where(p => p.EstadoId != idEliminado);

                var productos = await q.ToListAsync();

                var productosResponse = productos.Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.CodigoProducto,
                    p.MarcaProducto,
                    p.PrecioAdquisicion,
                    p.PrecioVenta,
                    p.Descripcion,
                    p.Imagen,
                    Estado = p.Estado.Nombre,
                    Categoria = p.Categoria != null ? p.Categoria.CategoriaNombre : null,
                    Proveedor = p.Proveedor != null ? p.Proveedor.Nombre : null,
                }).ToList();

                return Ok(productosResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al listar productos: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, $"Error interno al listar productos: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProducto(int id)
        {
            try
            {
                _logger.LogInformation($"Buscando producto con ID: {id}");
                var producto = await _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .Include(p => p.Estado)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                {
                    _logger.LogWarning($"Producto con ID {id} no encontrado");
                    return NotFound($"No se encontró el producto con ID {id}");
                }

                var productoResponse = new
                {
                    producto.Id,
                    producto.Nombre,
                    producto.CodigoProducto,
                    producto.MarcaProducto,
                    producto.PrecioAdquisicion,
                    producto.PrecioVenta,
                    producto.Descripcion,
                    Estado = producto.Estado.Nombre,
                    producto.Imagen,
                    Categoria = producto.Categoria != null ? producto.Categoria.CategoriaNombre : null,
                    Proveedor = producto.Proveedor != null ? producto.Proveedor.Nombre : null,
                };

                return Ok(productoResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener producto {id}: {ex.Message}");
                return StatusCode(500, "Error interno al obtener el producto");
            }
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<object>> PostProducto([FromBody] ProductoCreateDto productoDto)
        {
            try
            {
                _logger.LogInformation($"Creando producto: {productoDto.Nombre}");

                if (string.IsNullOrEmpty(productoDto.Nombre))
                    return BadRequest("El nombre del producto es obligatorio.");

                var idEliminado = await GetEstadoEliminadoIdAsync();

                // Validar que Categoría y Proveedor no estén 'Eliminado'
                if (await _context.Categorias.AnyAsync(c => c.Id == productoDto.CategoriaId && c.EstadoId == idEliminado))
                    return BadRequest("La categoría seleccionada está eliminada y no puede usarse.");
                if (await _context.Proveedores.AnyAsync(p => p.Id == productoDto.ProveedorId && p.EstadoId == idEliminado))
                    return BadRequest("El proveedor seleccionado está eliminado y no puede usarse.");

                var producto = new Productos
                {
                    Nombre = productoDto.Nombre,
                    CodigoProducto = productoDto.CodigoProducto,
                    MarcaProducto = productoDto.MarcaProducto,
                    PrecioAdquisicion = productoDto.PrecioAdquisicion,
                    PrecioVenta = productoDto.PrecioVenta,
                    Descripcion = productoDto.Descripcion,
                    Imagen = productoDto.Imagen,
                    EstadoId = productoDto.EstadoId,
                    CategoriaId = productoDto.CategoriaId,
                    ProveedorId = productoDto.ProveedorId,
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                var productoResponse = new
                {
                    producto.Id,
                    producto.Nombre,
                    producto.CodigoProducto,
                    producto.MarcaProducto,
                    producto.PrecioAdquisicion,
                    producto.PrecioVenta,
                    producto.Descripcion,
                    producto.Imagen,
                    producto.Estado,
                    producto.CategoriaId,
                    producto.ProveedorId,
                };

                return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, productoResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear producto: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, $"Error interno al crear el producto: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // PUT: api/Productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoUpdateDto productoDto)
        {
            try
            {
                if (id != productoDto.Id)
                {
                    _logger.LogWarning($"ID no coincide en actualización: {id} vs {productoDto.Id}");
                    return BadRequest("El ID no coincide con el producto a actualizar");
                }

                var productoExistente = await _context.Productos.FindAsync(id);
                if (productoExistente == null)
                {
                    _logger.LogWarning($"Producto no encontrado para actualización: {id}");
                    return NotFound($"No se encontró el producto con ID {id}");
                }

                var idEliminado = await GetEstadoEliminadoIdAsync();

                // Validar que Categoría y Proveedor no estén 'Eliminado'
                if (await _context.Categorias.AnyAsync(c => c.Id == productoDto.CategoriaId && c.EstadoId == idEliminado))
                    return BadRequest("La categoría seleccionada está eliminada y no puede usarse.");
                if (await _context.Proveedores.AnyAsync(p => p.Id == productoDto.ProveedorId && p.EstadoId == idEliminado))
                    return BadRequest("El proveedor seleccionado está eliminado y no puede usarse.");

                // Actualizar campos
                productoExistente.Nombre = productoDto.Nombre;
                productoExistente.CodigoProducto = productoDto.CodigoProducto;
                productoExistente.MarcaProducto = productoDto.MarcaProducto;
                productoExistente.PrecioAdquisicion = productoDto.PrecioAdquisicion;
                productoExistente.PrecioVenta = productoDto.PrecioVenta;
                productoExistente.Descripcion = productoDto.Descripcion;
                productoExistente.EstadoId = productoDto.EstadoId;
                productoExistente.Imagen = productoDto.Imagen;
                productoExistente.CategoriaId = productoDto.CategoriaId;
                productoExistente.ProveedorId = productoDto.ProveedorId;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Producto actualizado correctamente: {id}");

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"Error de concurrencia al actualizar producto {id}: {ex.Message}");
                return StatusCode(409, "Error de concurrencia al actualizar el producto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar producto: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, $"Error interno al actualizar el producto: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // DELETE: api/Productos/5  (Soft delete + no borrar si está en uso)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    _logger.LogWarning($"Intento de eliminar producto inexistente: {id}");
                    return NotFound($"No se encontró el producto con ID {id}");
                }

                // 1) Verificar si el producto está en uso (ajusta el DbSet/nombre de FK si difiere)
                bool enUso = await _context.Pedido.AnyAsync(d => d.ProductoId == id);
                // Si tu tabla se llama distinto (PedidoDetalles/DetallePedido/PedidoProductos), cambia esta línea.

                if (enUso)
                {
                    _logger.LogWarning($"No se puede eliminar producto {id} porque está en uso.");
                    return Conflict("No se puede eliminar el producto porque está en uso.");
                }

                // 2) Soft delete: cambiar EstadoId a 'Eliminado'
                var idEliminado = await GetEstadoEliminadoIdAsync();
                if (idEliminado == 0)
                {
                    _logger.LogError("Estado 'Eliminado' no configurado en la tabla Estados.");
                    return StatusCode(500, "No está configurado el estado 'Eliminado' en la tabla de Estados.");
                }

                if (producto.EstadoId != idEliminado)
                {
                    producto.EstadoId = idEliminado; // Soft delete
                    _context.Entry(producto).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Producto marcado como Eliminado: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar (soft delete) producto {id}: {ex.Message}");
                return StatusCode(500, "Error interno al eliminar el producto");
            }
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(p => p.Id == id);
        }
    }
}
