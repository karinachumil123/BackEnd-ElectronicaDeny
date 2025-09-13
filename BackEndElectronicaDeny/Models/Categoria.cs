using BackEnd_ElectronicaDeny.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BackEndElectronicaDeny.Models
{
    public class Categoria
    {
        public Categoria()
        {
            Productos = new HashSet<Productos>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string CategoriaNombre { get; set; }

        public string Descripcion { get; set; }

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
