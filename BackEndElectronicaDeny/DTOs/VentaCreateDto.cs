using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BackEndElectronicaDeny.DTOs.TiendaKeytlin.Server.DTOs
{
    public class VentaCreateDto
    {
        /// Cliente opcional. Si no se envía o es 0, se considera sin cliente.
        public int? ClienteId { get; set; }

        [Required]
        public decimal MontoRecibido { get; set; }

        public string? Observaciones { get; set; }

        /// Id del usuario que registra la venta
        [Required]
        public int VendedorId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un item")]
        public List<DetalleVentaCreateDto> DetallesVenta { get; set; } = new();
    }

    public class DetalleVentaCreateDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required, Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
        public decimal PrecioUnitario { get; set; }
    }

    // Responses (sin cambios funcionales)
    public class VentaResponseDto
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal Cambio { get; set; }
        public int Cantidad { get; set; }
        public string? Observaciones { get; set; }
        public VendedorDto Vendedor { get; set; } = new();
        public int? ClienteId { get; set; }           // <— nullable
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string? ClienteDireccion { get; set; }
        public List<DetalleVentaResponseDto> DetallesVenta { get; set; } = new();
    }

    public class DetalleVentaResponseDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string CodigoProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class VendedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
    }

    public class ReciboVentaDto
    {
        public int VentaId { get; set; }
        public DateTime FechaVenta { get; set; }

        public string DireccionTienda { get; set; } = string.Empty;
        public string TelefonoEmpresa { get; set; } = "";

        public string NombreVendedor { get; set; } = string.Empty;
        public string EmailVendedor { get; set; } = string.Empty;
        public string TelefonoVendedor { get; set; } = string.Empty;

        public int? ClienteId { get; set; }           // <— nullable
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string? ClienteDireccion { get; set; }
        public string? ClienteNit { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal Cambio { get; set; }

        public List<DetalleVentaResponseDto> Productos { get; set; } = new();
    }
}
