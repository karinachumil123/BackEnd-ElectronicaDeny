using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.DTOs;
using BackEndElectronicaDeny.Models;
using BackEndElectronicaDeny.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly InventoryService _inv;

        public StockController(AppDbContext db, InventoryService inv)
        {
            _db = db;
            _inv = inv;
        }

        // GET /api/stock  -> para la tabla del Front
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockListItemDto>>> Get()
        {
            var data = await _db.Stock
                .Include(s => s.Producto)
                .AsNoTracking()
                .Select(s => new StockListItemDto
                {
                    ProductoId = s.ProductoId,
                    CodigoProducto = s.Producto.CodigoProducto,
                    Nombre = s.Producto.Nombre,
                    StockDisponible = s.StockDisponible,
                    StockMinimo = s.StockMinimo,
                    NombreCategoria = s.NombreCategoria,
                    PrecioVenta = s.PrecioVenta,
                    PrecioCompra = s.PrecioCompra,
                    Imagen = s.Producto.Imagen
                })
                .ToListAsync();

            return Ok(data);
        }

        // POST /api/stock/from-pedido
        [HttpPost("from-pedido")]
        public async Task<IActionResult> FromPedido([FromBody] FromPedidoDto dto)
        {
            if (dto?.Items == null || dto.Items.Count == 0)
                return BadRequest("Sin items.");

            // Consolidar por producto: suma cantidad y el último precioCompra
            var map = new Dictionary<int, (int cantidad, decimal precioCompra)>();
            foreach (var i in dto.Items)
            {
                var cant = Math.Max(0, i.Cantidad);
                if (map.TryGetValue(i.ProductoId, out var prev))
                    map[i.ProductoId] = (prev.cantidad + cant, i.PrecioCompra);
                else
                    map[i.ProductoId] = (cant, i.PrecioCompra);
            }

            // 1) aplicar movimientos en memoria
            await _inv.UpsertStockAsync(map);

            // 2) un solo guardado
            await _db.SaveChangesAsync();

            return Ok(new { ok = true });
        }

        // PUT /api/stock/{productoId}
        [HttpPut("{productoId:int}")]
        public async Task<IActionResult> Update(int productoId, [FromBody] UpdateStockDto dto)
        {
            var stock = await _db.Stock.FirstOrDefaultAsync(s => s.ProductoId == productoId);
            if (stock == null) return NotFound("No existe stock para el producto.");

            // 1) actualizar inventario
            stock.StockDisponible = Math.Max(0, dto.StockDisponible);
            stock.StockMinimo = Math.Max(0, dto.StockMinimo);
            stock.PrecioVenta = dto.PrecioVenta;

            // 2) espejar a Productos (precio de venta) usando el servicio
            await _inv.SyncProductPriceFromStockAsync(productoId, dto.PrecioVenta);

            // 3) un solo guardado para stock + producto
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
