using BackEndElectronicaDeny.Dtos;
using BackEndElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc;
using BackEnd_ElectronicaDeny.Data;
using Microsoft.EntityFrameworkCore;

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

        // POST: api/pedidos
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CrearPedidoDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var pedido = new Pedido
            {
                NumeroPedido = dto.NumeroPedido!,
                NombrePedido = dto.NombrePedido!,
                FechaPedido = DateTime.UtcNow.Date,
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

        // POST: api/pedidos/{id}/enviar-a-inventario
        // Cambia estado a Inventariado.
        // (A futuro aquí puedes crear movimientos de inventario)
        [HttpPost("{id:int}/enviar-a-inventario")]
        public async Task<ActionResult> EnviarAInventario(int id, [FromBody] EnviarAInventarioDto body)
        {
            var pedido = await _db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return NotFound();

            // Reglas: Debe estar Recibido
            if (pedido.EstadoPedidoId != (int)EstadoPedido.Recibido)
                return BadRequest(new { message = "El pedido debe estar en estado 'Recibido' para enviarlo a inventario." });

            // TODO: crear movimientos de inventario aquí (cuando tengas el módulo)
            // Ejemplo: foreach (var d in pedido.Detalles) { ... }

            pedido.EstadoPedidoId = (int)EstadoPedido.Inventariado;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Pedido enviado a Inventario.", pedidoId = pedido.Id });
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

