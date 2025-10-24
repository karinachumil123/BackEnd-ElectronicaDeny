namespace BackEndElectronicaDeny.DTOs
{
    public class CierreCajaCreateDto
    {
        public DateTime? FechaCierreUtc { get; set; }  
        public string? Descripcion { get; set; }       
        public List<ClasificacionDto>? Clasificaciones { get; set; } 
    }

    public class ClasificacionDto
    {
        public string? Denominacion { get; set; }
        public decimal Valor { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }          
    }

}
