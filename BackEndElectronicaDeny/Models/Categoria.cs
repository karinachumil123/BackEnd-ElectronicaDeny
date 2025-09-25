using BackEndElectronicaDeny.Models;
using System.ComponentModel.DataAnnotations;
using BackEnd_ElectronicaDeny.Models;
using System.Text.Json.Serialization;

public class Categoria
{
    public Categoria()
    {
        Productos = new HashSet<Productos>();
    }

    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string CategoriaNombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    /// <summary>1 = Activo, 2 = Inactivo</summary>
    [Required]
    [Range(1, 2, ErrorMessage = "EstadoId solo puede ser 1 (Activo) o 2 (Inactivo).")]
    public int EstadoId { get; set; } = 1;

    [JsonIgnore]
    public virtual ICollection<Productos> Productos { get; set; }
}

