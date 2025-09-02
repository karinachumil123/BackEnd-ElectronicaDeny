namespace BackEnd_ElectronicaDeny.DTOs
{
    public class UsuarioCreateDto
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Correo { get; set; }
        public string Imagen { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int edad { get; set; }
        public string Contrasena { get; set; }
        public string Telefono { get; set; }
        public int EstadoId { get; set; }
        public int RolId { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
