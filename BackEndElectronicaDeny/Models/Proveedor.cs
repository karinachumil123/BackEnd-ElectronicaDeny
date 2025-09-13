using BackEnd_ElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndElectronicaDeny.Models
{
    public class Proveedor
    {
        public Proveedor()
        {
            Productos = new HashSet<Productos>();
        }

        public int Id { get; set; }
        public string Nombre { get; set; } // Cambiado de NombreEmpresa a Nombre
        public string NombreContacto { get; set; }
        public string Telefono { get; set; }
        public string TelefonoContacto { get; set; }
        public string Correo { get; set; } // Agregado para coincidir con frontend
        public string? Direccion { get; set; } // Opcional
        public string? Descripcion { get; set; } // Opcional

        [Required]
        public int EstadoId { get; set; }

        [JsonIgnore]
        [ValidateNever] // <- clave para evitar validación automática del objeto anidado
        [ForeignKey("EstadoId")]
        public virtual Estados Estado { get; set; }

        [JsonIgnore]
        public virtual ICollection<Productos> Productos { get; set; }
    }
}
