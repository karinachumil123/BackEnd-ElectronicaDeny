namespace BackEndElectronicaDeny.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string? Correo { get; set; }
        public string Telefono { get; set; } = null!;
        public string? NIT { get; set; }
        public string? Direccion { get; set; }
        public string? Municipio { get; set; }
        public string? Departamento { get; set; }
        public int Estado { get; set; }

        public string FechaRegistroTexto { get; set; } = "";
    }
}
