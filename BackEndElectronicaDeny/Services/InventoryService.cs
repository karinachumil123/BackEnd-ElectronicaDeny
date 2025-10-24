using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackEnd_ElectronicaDeny.Data;
using BackEndElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEndElectronicaDeny.Services
{
    public class InventoryService
    {
        private const int StockImagenMaxLen = 500;
        private readonly AppDbContext _db;

        public InventoryService(AppDbContext db) => _db = db;

        // --- Helper centralizado para 'Stock.Imagen' ----
        private static string? SanitizeImageForStock(string? imagen)
        {
            if (string.IsNullOrWhiteSpace(imagen)) return null;

            // Si viene un data-URL/base64: no lo guardes en Stock (evita overflow y duplicación)
            if (imagen.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return null;

            // Si parece ruta/URL, intenta quedarte con el nombre de archivo
            try
            {
                var file = Path.GetFileName(imagen);
                if (!string.IsNullOrWhiteSpace(file) && file.Length <= StockImagenMaxLen)
                    return file;
            }
            catch { /* ignorar y seguir */ }

            // Si aún es largo, recorta a 500 (último recurso)
            return imagen.Length > StockImagenMaxLen ? imagen[..StockImagenMaxLen] : imagen;
        }

        /// <summary>
        /// Descuenta stock por venta. Lanza InvalidOperationException si no hay stock suficiente
        /// o si no existe el registro de stock. **NO** hace SaveChanges: el caller guarda/abre transacción.
        /// </summary>
        public async Task DescontarPorVentaAsync(
            Dictionary<int, int> itemsPorProducto,
            CancellationToken ct = default)
        {
            if (itemsPorProducto == null || itemsPorProducto.Count == 0) return;

            var ids = itemsPorProducto.Keys.Distinct().ToList();

            var stocks = await _db.Stock
                .Where(s => ids.Contains(s.ProductoId))
                .ToDictionaryAsync(s => s.ProductoId, ct);

            foreach (var (productoId, cantidad) in itemsPorProducto)
            {
                if (cantidad <= 0)
                    throw new InvalidOperationException($"Cantidad inválida para el producto {productoId}.");

                if (!stocks.TryGetValue(productoId, out var stock))
                    throw new InvalidOperationException($"No existe registro de stock para el producto {productoId}.");

                if (stock.StockDisponible < cantidad)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para el producto {productoId}. Disponible: {stock.StockDisponible}, solicitado: {cantidad}");

                stock.StockDisponible -= cantidad;
                if (stock.StockDisponible < 0) stock.StockDisponible = 0;
            }
            // NO SaveChanges aquí
        }

        /// <summary>
        /// Upsert de stock a partir de movimientos ProductoId -> (Cantidad, PrecioCompra).
        /// **NO** hace SaveChanges: el caller guarda.
        /// </summary>
        public async Task UpsertStockAsync(
            Dictionary<int, (int cantidad, decimal precioCompra)> movimientos,
            CancellationToken ct = default)
        {
            if (movimientos == null || movimientos.Count == 0) return;

            var ids = movimientos.Keys.Distinct().ToList();

            var productos = await _db.Productos
                .Where(p => ids.Contains(p.Id))
                .Include(p => p.Categoria)
                .AsNoTracking()
                .ToDictionaryAsync(p => p.Id, ct);

            var stocks = await _db.Stock
                .Where(s => ids.Contains(s.ProductoId))
                .ToDictionaryAsync(s => s.ProductoId, ct);

            foreach (var (productoId, tuple) in movimientos)
            {
                var (cantidad, precioCompra) = tuple;

                if (!productos.TryGetValue(productoId, out var prod))
                    continue;

                if (!stocks.TryGetValue(productoId, out var s))
                {
                    s = new Stock
                    {
                        ProductoId = productoId,
                        StockDisponible = 0,
                        StockMinimo = 0,
                        PrecioCompra = 0,
                        PrecioVenta = prod.PrecioVenta,
                        Imagen = SanitizeImageForStock(prod.Imagen),
                        NombreCategoria = prod.Categoria?.CategoriaNombre ?? string.Empty
                    };
                    _db.Stock.Add(s);
                    stocks[productoId] = s;
                }

                s.StockDisponible += Math.Max(0, cantidad);
                s.PrecioCompra = precioCompra;                       // del detalle
                s.PrecioVenta = prod.PrecioVenta;                    // espejo
                s.Imagen = SanitizeImageForStock(prod.Imagen);       // espejo saneado
                s.NombreCategoria = prod.Categoria?.CategoriaNombre ?? s.NombreCategoria;
            }
            // NO SaveChanges aquí
        }

        /// <summary>
        /// Procesa un pedido completo: lee Detalles y aplica UpsertStockAsync.
        /// **NO** hace SaveChanges: el caller guarda.
        /// </summary>
        public async Task ProcesarPedidoEnInventarioAsync(int pedidoId, CancellationToken ct = default)
        {
            var pedido = await _db.Pedidos
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == pedidoId, ct);

            if (pedido == null) throw new InvalidOperationException($"Pedido {pedidoId} no existe.");
            if (pedido.Detalles == null || pedido.Detalles.Count == 0)
                throw new InvalidOperationException("El pedido no tiene detalles.");

            var map = new Dictionary<int, (int cantidad, decimal precioCompra)>();
            foreach (var d in pedido.Detalles)
            {
                var cant = Math.Max(0, d.Cantidad);
                var precio = d.PrecioUnitario; // costo desde Detalle.PrecioUnitario

                if (map.TryGetValue(d.ProductoId, out var prev))
                    map[d.ProductoId] = (prev.cantidad + cant, precio);
                else
                    map[d.ProductoId] = (cant, precio);
            }

            await UpsertStockAsync(map, ct);
            // NO SaveChanges aquí
        }

        /// <summary>
        /// Espejo Producto->Stock al crear/actualizar producto. Si no hay Stock, lo crea.
        /// **NO** hace SaveChanges: el caller guarda.
        /// </summary>
        public async Task SyncStockFromProductAsync(int productoId, CancellationToken ct = default)
        {
            var prod = await _db.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == productoId, ct);
            if (prod == null) return;

            var stock = await _db.Stock.FirstOrDefaultAsync(s => s.ProductoId == productoId, ct);
            var img = SanitizeImageForStock(prod.Imagen);

            if (stock == null)
            {
                stock = new Stock
                {
                    ProductoId = productoId,
                    StockDisponible = 0,
                    StockMinimo = 0,
                    PrecioCompra = 0,
                    PrecioVenta = prod.PrecioVenta,
                    Imagen = img,
                    NombreCategoria = prod.Categoria?.CategoriaNombre ?? string.Empty
                };
                _db.Stock.Add(stock);
            }
            else
            {
                stock.PrecioVenta = prod.PrecioVenta;
                stock.Imagen = img;
                stock.NombreCategoria = prod.Categoria?.CategoriaNombre ?? stock.NombreCategoria;
            }
            // NO SaveChanges aquí
        }

        /// <summary>
        /// Desde Inventario -> sincroniza Productos.PrecioVenta.
        /// **NO** hace SaveChanges: el caller guarda.
        /// </summary>
        public async Task SyncProductPriceFromStockAsync(int productoId, decimal nuevoPrecioVenta, CancellationToken ct = default)
        {
            var prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == productoId, ct);
            if (prod == null) return;
            prod.PrecioVenta = nuevoPrecioVenta;
            // NO SaveChanges aquí
        }
    }
}
