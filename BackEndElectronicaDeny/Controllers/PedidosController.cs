using BackEndElectronicaDeny.Dtos;
using BackEndElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using BackEnd_ElectronicaDeny.Data;
using Microsoft.EntityFrameworkCore;
using BackEndElectronicaDeny.Services;

namespace BackEndElectronicaDeny.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PedidosController(AppDbContext db) { _db = db; }

        // GET: api/pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll()
        {
            var data = await _db.Pedidos
                .Include(p => p.Proveedor)
                .Include(p => p.Detalles)
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Select(p => new {
                    p.Id,
                    p.NumeroPedido,
                    p.NombrePedido,
                    p.FechaPedido,
                    p.ProveedorId,
                    Estado = new { Id = p.EstadoPedidoId, Nombre = EstadoNombre(p.EstadoPedidoId) },
                    Proveedor = p.Proveedor == null ? null : new { p.Proveedor.Id, p.Proveedor.Nombre },
                    p.Descripcion,
                    p.TotalPedido,
                    Detalles = p.Detalles.Select(d => new {
                        d.Id,
                        d.ProductoId,
                        d.Cantidad,
                        d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    })
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/pedidos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var p = await _db.Pedidos
                .Include(x => x.Proveedor)
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (p == null) return NotFound();

            var result = new
            {
                p.Id,
                p.NumeroPedido,
                p.NombrePedido,
                p.FechaPedido,
                p.ProveedorId,
                Estado = new { Id = p.EstadoPedidoId, Nombre = EstadoNombre(p.EstadoPedidoId) },
                Proveedor = p.Proveedor == null ? null : new { p.Proveedor.Id, p.Proveedor.Nombre },
                p.Descripcion,
                p.TotalPedido,
                Detalles = p.Detalles.Select(d => new {
                    d.Id,
                    d.ProductoId,
                    d.Cantidad,
                    d.PrecioUnitario,
                    Subtotal = d.Cantidad * d.PrecioUnitario
                })
            };
            return Ok(result);
        }
        
        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetTotalPedidosDelDia([FromQuery] DateTime? fecha)
        {
            if (fecha is null) return BadRequest("fecha requerida (yyyy-MM-dd)");

            // Rango del día (local) -> convierte a UTC si FechaPedido está guardada en UTC
            var fromLocal = fecha.Value.Date;
            var toLocal = fromLocal.AddDays(1);

            var fromUtc = DateTime.SpecifyKind(fromLocal, DateTimeKind.Local).ToUniversalTime();
            var toUtc = DateTime.SpecifyKind(toLocal, DateTimeKind.Local).ToUniversalTime();

            // Estados que SÍ cuentan como salida (recomendado)
            int RECIBIDO = 3;
            int INVENTARIADO = 5;

            var total = await _db.Pedidos
                .Where(p =>
                    p.FechaPedido >= fromUtc && p.FechaPedido < toUtc &&
                    (p.EstadoPedidoId == RECIBIDO || p.EstadoPedidoId == INVENTARIADO))
                .SumAsync(p => (decimal?)p.TotalPedido) ?? 0m;

            return Ok(total);
        }

        // POST: api/pedidos
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CrearPedidoDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var pedido = new Pedido
            {
                NumeroPedido = dto.NumeroPedido!,
                NombrePedido = dto.NombrePedido!,
                FechaPedido = DateTime.UtcNow,
                ProveedorId = dto.ProveedorId,
                EstadoPedidoId = dto.EstadoPedidoId,
                Descripcion = dto.Descripcion,
                TotalPedido = dto.TotalPedido ?? 0m,
            };

            foreach (var d in dto.Detalles)
            {
                pedido.Detalles.Add(new DetallePedido
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                });
                pedido.TotalPedido += d.Cantidad * d.PrecioUnitario;
            }

            _db.Pedidos.Add(pedido);
            await _db.SaveChangesAsync();

            // Devuelve lo que realmente quedó en BD (incluida la fecha)
            return CreatedAtAction(nameof(GetById), new { id = pedido.Id }, new
            {
                pedido.Id,
                pedido.NumeroPedido,
                pedido.NombrePedido,
                pedido.FechaPedido,
                pedido.ProveedorId,
                pedido.EstadoPedidoId,
                pedido.Descripcion,
                pedido.TotalPedido
            });
        }

        // PUT: api/pedidos/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] ActualizarPedidoDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var pedido = await _db.Pedidos
                                  .Include(p => p.Detalles)
                                  .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();

            pedido.NumeroPedido = dto.NumeroPedido!;
            pedido.NombrePedido = dto.NombrePedido!;
            pedido.ProveedorId = dto.ProveedorId;
            pedido.EstadoPedidoId = dto.EstadoPedidoId;
            pedido.Descripcion = dto.Descripcion;
            pedido.TotalPedido = 0m;

            // Reemplazar detalles
            _db.DetallePedidos.RemoveRange(pedido.Detalles);
            pedido.Detalles.Clear();

            foreach (var d in dto.Detalles)
            {
                var det = new DetallePedido
                {
                    PedidoId = pedido.Id,
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                };
                pedido.Detalles.Add(det);
                pedido.TotalPedido += d.Cantidad * d.PrecioUnitario;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/pedidos/eliminar-logico/5  (marca Cancelado)
        [HttpPut("eliminar-logico/{id:int}")]
        public async Task<ActionResult> EliminarLogico(int id)
        {
            var pedido = await _db.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.EstadoPedidoId = (int)EstadoPedido.Cancelado;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/pedidos/{id}/mandar-a-inventario
        [HttpPost("{id:int}/mandar-a-inventario")]
        public async Task<ActionResult> MandarAInventario(int id, [FromServices] InventoryService inv)
        {
            var pedido = await _db.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound(new { message = $"Pedido {id} no existe." });
            if (pedido.Detalles == null || !pedido.Detalles.Any())
                return BadRequest(new { message = "El pedido no tiene detalles." });

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                await inv.ProcesarPedidoEnInventarioAsync(id); // upsert stock (con SaveChanges)
                pedido.EstadoPedidoId = (int)EstadoPedido.Inventariado;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(new { ok = true, message = "Pedido inventariado. Stock actualizado.", pedidoId = pedido.Id });
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                var inner = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = $"DB error al mandar a inventario: {inner}" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, new { message = $"Error al mandar a inventario: {ex.Message}" });
            }
        }

        // PUT: api/pedidos/{id}/eliminar
        [HttpPut("{id:int}/eliminar")]
        public async Task<ActionResult> MarcarEliminado(int id)
        {
            var pedido = await _db.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.EstadoPedidoId = (int)EstadoPedido.Eliminado;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private static string EstadoNombre(int id) => id switch
        {
            1 => "Pendiente",
            2 => "Enviado",
            3 => "Recibido",
            4 => "Cancelado",
            5 => "Inventariado",
            6 => "Eliminado",
            _ => "Desconocido"
        };
    }

    // Opcional: Enum de estados para tipar mejor
    public enum EstadoPedido
    {
        Pendiente = 1,
        Enviado = 2,
        Recibido = 3,
        Cancelado = 4,
        Inventariado = 5,
        Eliminado = 6
    }
}

