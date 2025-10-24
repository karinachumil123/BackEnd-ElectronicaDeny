namespace BackEndElectronicaDeny.DTOs
{
    public class AperturaCajaCreateDto
    {
        public DateTime FechaAperturaUtc { get; set; }
        public decimal MontoApertura { get; set; }
        public string? Notas { get; set; }
        public string? CajeroNombre { get; set; }
    }
}
