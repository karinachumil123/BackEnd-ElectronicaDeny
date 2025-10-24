using BackEnd_ElectronicaDeny.Data;        
using BackEndElectronicaDeny.DTOs;
using BackEndElectronicaDeny.Models;
using BackEndElectronicaDeny.Services;
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
        private readonly InventoryService _inv;

        public ProductoController(AppDbContext context, ILogger<ProductoController> logger, InventoryService inv)
        {
            _context = context;
            _logger = logger;
            _inv = inv;
        }

        private static string EstadoNombre(int estadoId) =>
            estadoId == 1 ? "Activo" :
            estadoId == 2 ? "Inactivo" : "Desconocido";

        private static bool EsActivo(int estadoId) => estadoId == 1;
        private static bool EsInactivo(int estadoId) => estadoId == 2;

        // GET: api/Producto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProductos(
            [FromQuery] bool? incluirInactivos = null,
            [FromQuery] bool? incluirEliminados = null  
        )
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de productos");

                bool incluir = (incluirInactivos ?? false) || (incluirEliminados ?? false);

                var q = _context.Productos
                    .Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .AsNoTracking();

                if (!incluir)
                {
                    // Solo activos por defecto
                    q = q.Where(p => p.EstadoId == 1);
                }

                var productos = await q.ToListAsync();

                var response = productos.Select(p => new
                {
                    p.Id,
                    p.Nombre,
                    p.CodigoProducto,
                    MarcaProducto = p.MarcaProducto,
                    p.PrecioAdquisicion,
                    p.PrecioVenta,
                    p.Descripcion,
                    p.Imagen,
                    Estado = EstadoNombre(p.EstadoId),
                    p.EstadoId,
                    Categoria = p.Categoria != null ? p.Categoria.CategoriaNombre : null,
                    Proveedor = p.Proveedor != null ? p.Proveedor.Nombre : null,
                    p.CategoriaId,
                    p.ProveedorId
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar productos");
                return StatusCode(500, "Error interno al listar productos");
            }
        }

        // ==========================================================
        // GET: api/Producto/por-proveedor/{proveedorId}
        // Solo productos ACTIVOS (EstadoId == 1) del proveedor dado
        // Útil para el combo del modal al seleccionar proveedor
        // ==========================================================
        [HttpGet("por-proveedor/{proveedorId:int}")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductosActivosPorProveedor(int proveedorId)
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.ProveedorId == proveedorId && p.EstadoId == 1) // solo activos
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.Id,
                        p.Nombre,
                        p.ProveedorId
                    })
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar productos por proveedor {ProveedorId}", proveedorId);
                return StatusCode(500, "Error interno al listar productos por proveedor");
            }
        }

        // GET: api/Producto/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetProducto(int id)
        {
            try
            {
                _logger.LogInformation("Buscando producto {Id}", id);

                var p = await _context.Productos
                    .Include(x => x.Categoria)
                    .Include(x => x.Proveedor)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (p == null) return NotFound($"No se encontró el producto con ID {id}");

                var response = new
                {
                    p.Id,
                    p.Nombre,
                    p.CodigoProducto,
                    MarcaProducto = p.MarcaProducto,
                    p.PrecioAdquisicion,
                    p.PrecioVenta,
                    p.Descripcion,
                    p.Imagen,
                    Estado = EstadoNombre(p.EstadoId),
                    p.EstadoId,
                    Categoria = p.Categoria?.CategoriaNombre,
                    Proveedor = p.Proveedor?.Nombre,
                    p.CategoriaId,
                    p.ProveedorId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {Id}", id);
                return StatusCode(500, "Error interno al obtener el producto");
            }
        }

        // POST: api/Producto
        [HttpPost]
        public async Task<ActionResult<object>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    return BadRequest("El nombre del producto es obligatorio.");

                var categoriaActiva = await _context.Categorias
                    .AnyAsync(c => c.Id == dto.CategoriaId && c.EstadoId == 1);
                if (!categoriaActiva)
                    return BadRequest("La categoría seleccionada no está activa y no puede usarse.");

                var proveedorActivo = await _context.Proveedores
                    .AnyAsync(p => p.Id == dto.ProveedorId && p.EstadoId == 1);
                if (!proveedorActivo)
                    return BadRequest("El proveedor seleccionado no está activo y no puede usarse.");

                var estadoId = (dto.EstadoId == 1 || dto.EstadoId == 2) ? dto.EstadoId : 1;

                var producto = new Productos
                {
                    Nombre = dto.Nombre,
                    CodigoProducto = dto.CodigoProducto,
                    MarcaProducto = dto.MarcaProducto,
                    PrecioAdquisicion = dto.PrecioAdquisicion,
                    PrecioVenta = dto.PrecioVenta,
                    Descripcion = dto.Descripcion,
                    Imagen = dto.Imagen,
                    EstadoId = estadoId,
                    CategoriaId = dto.CategoriaId,
                    ProveedorId = dto.ProveedorId
                };

                // 1) guarda el producto (necesitamos el Id)
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                // 2) crea/actualiza el espejo en Stock (NO guarda por dentro)
                await _inv.SyncStockFromProductAsync(producto.Id);

                // 3) un segundo guardado para persistir el Stock creado/ajustado
                await _context.SaveChangesAsync();

                var response = new
                {
                    producto.Id,
                    producto.Nombre,
                    producto.CodigoProducto,
                    producto.MarcaProducto,
                    producto.PrecioAdquisicion,
                    producto.PrecioVenta,
                    producto.Descripcion,
                    producto.Imagen,
                    Estado = EstadoNombre(producto.EstadoId),
                    producto.EstadoId,
                    producto.CategoriaId,
                    producto.ProveedorId
                };

                return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, "Error interno al crear el producto");
            }
        }

        // PUT: api/Producto/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProducto(int id, [FromBody] ProductoUpdateDto dto)
        {
            try
            {
                if (id != dto.Id) return BadRequest("El ID no coincide con el producto a actualizar.");

                var p = await _context.Productos.FindAsync(id);
                if (p == null) return NotFound($"No se encontró el producto con ID {id}");

                var categoriaActiva = await _context.Categorias
                    .AnyAsync(c => c.Id == dto.CategoriaId && c.EstadoId == 1);
                if (!categoriaActiva)
                    return BadRequest("La categoría seleccionada no está activa y no puede usarse.");

                var proveedorActivo = await _context.Proveedores
                    .AnyAsync(pr => pr.Id == dto.ProveedorId && pr.EstadoId == 1);
                if (!proveedorActivo)
                    return BadRequest("El proveedor seleccionado no está activo y no puede usarse.");

                var estadoId = (dto.EstadoId == 1 || dto.EstadoId == 2) ? dto.EstadoId : p.EstadoId;

                // 1) actualizar producto
                p.Nombre = dto.Nombre;
                p.CodigoProducto = dto.CodigoProducto;
                p.MarcaProducto = dto.MarcaProducto;
                p.PrecioAdquisicion = dto.PrecioAdquisicion;
                p.PrecioVenta = dto.PrecioVenta;
                p.Descripcion = dto.Descripcion;
                p.Imagen = dto.Imagen;
                p.EstadoId = estadoId;
                p.CategoriaId = dto.CategoriaId;
                p.ProveedorId = dto.ProveedorId;

                // 2) espejar a Stock usando el servicio (NO guarda por dentro)
                await _inv.SyncStockFromProductAsync(id);

                // 3) un solo guardado para producto + stock
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrencia al actualizar producto {Id}", id);
                return StatusCode(409, "Error de concurrencia al actualizar el producto");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {Id}", id);
                return StatusCode(500, "Error interno al actualizar el producto");
            }
        }

        // DELETE: api/Producto/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            try
            {
                var p = await _context.Productos.FindAsync(id);
                if (p == null) return NotFound($"No se encontró el producto con ID {id}");

                // ¿Está en uso? (usa DetallePedidos, no Pedido)
                bool enUso = await _context.DetallePedidos.AnyAsync(d => d.ProductoId == id);
                if (enUso)
                    return Conflict("No se puede inactivar el producto porque está en uso en pedidos.");

                // Soft delete → Inactivo (2)
                if (!EsInactivo(p.EstadoId))
                {
                    p.EstadoId = 2;
                    _context.Entry(p).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar (inactivar) producto {Id}", id);
                return StatusCode(500, "Error interno al eliminar el producto");
            }
        }

        private bool ProductoExists(int id) => _context.Productos.Any(p => p.Id == id);
    }
}

