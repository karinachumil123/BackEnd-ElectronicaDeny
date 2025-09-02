using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Models;
using BackEnd_ElectronicaDeny.Services;
using BackEnd_ElectronicaDeny.DTOs;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace BackEnd_ElectronicaDeny.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(
            AppDbContext context,
            EmailService emailService,
            ILogger<UsuarioController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            try
            {
                _logger.LogInformation("Obteniendo lista de usuarios");
                var usuarios = await _context.Usuarios
                    .Where(u => u.EstadoId == 1 || u.EstadoId == 2)
                    .Include(u => u.Estado)
                    .Include(u => u.Rol)
                    .ToListAsync();

                var usuariosResponse = usuarios.Select(usuario => new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Correo,
                    usuario.Imagen,
                    usuario.FechaNacimiento,
                    edad = CalcularEdad(usuario.FechaNacimiento ?? DateTime.Now),
                    usuario.Telefono,
                    usuario.FechaCreacion,
                    usuario.UltimoInicioSesion,
                    Estado = usuario.Estado.Nombre,
                    Rol = usuario.Rol.Nombre,
                    usuario.RolId
                }).ToList();

                return Ok(usuariosResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuarios: {ex.Message}");
                return StatusCode(500, "Error interno al obtener la lista de usuarios");
            }
        }

        // Método para calcular la edad basado en la fecha de nacimiento
        private int CalcularEdad(DateTime FechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;

           if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

            return edad;
        }

        // GET: api/usuario/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            try
            {
                _logger.LogInformation($"Buscando usuario con ID: {id}");
                var usuario = await _context.Usuarios
                    .Include(u => u.Estado)
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    _logger.LogWarning($"Usuario con ID {id} no encontrado");
                    return NotFound($"No se encontró el usuario con ID {id}");
                }

                return usuario;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuario {id}: {ex.Message}");
                return StatusCode(500, "Error interno al obtener el usuario");
            }
        }


        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> DescargarReportePdf()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Estado)
                    .Include(u => u.Rol)
                    .Where(u => u.EstadoId == 1 || u.EstadoId == 2)
                    .ToListAsync();

                // Declara el MemoryStream fuera de los bloques using anidados para usarlo al final
                var stream = new MemoryStream();

                using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf))
                {
                    // Fuentes
                    PdfFont font = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    PdfFont boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Título
                    document.Add(new iText.Layout.Element.Paragraph("Reporte de Usuarios")
                        .SetFont(boldFont)
                        .SetFontSize(18)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    // Tabla
                    var table = new iText.Layout.Element.Table(8).UseAllAvailableWidth();

                    // Encabezados
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("ID").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Email").SetFont(boldFont)));
                    // ... (resto de encabezados)

                    // Datos
                    foreach (var u in usuarios)
                    {
                        table.AddCell(u.Id.ToString());
                        table.AddCell(u.Correo ?? "-");
                        // ... (resto de celdas)
                    }

                    document.Add(table);
                    document.Close(); // Importante cerrar el documento
                }

                // Retorna el stream como File (el stream se libera automáticamente por FileResult)
                return File(stream.ToArray(), "application/pdf", "Reporte_Usuarios.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF: " + ex.ToString());
                return StatusCode(500, "Error interno al generar el PDF: " + ex.Message);
            }
        }


        // POST: api/usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario([FromBody] UsuarioCreateDto usuarioDto)
        {
            try
            {
                _logger.LogInformation($"Iniciando creación de usuario: {usuarioDto.Correo}");

                if (string.IsNullOrEmpty(usuarioDto.Correo))
                {
                    _logger.LogWarning("Intento de crear usuario sin correo electrónico");
                    return BadRequest("El correo electrónico es obligatorio.");
                }

                if (string.IsNullOrEmpty(usuarioDto.Contrasena))
                {
                    _logger.LogWarning($"Intento de crear usuario {usuarioDto.Correo} sin contraseña");
                    return BadRequest("La contraseña es obligatoria.");
                }

                if (string.IsNullOrEmpty(usuarioDto.Nombre) || string.IsNullOrEmpty(usuarioDto.Apellido))
                {
                    return BadRequest("El nombre y apellido son obligatorios.");
                }

                var usuarioExistente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == usuarioDto.Correo);

                if (usuarioExistente != null)
                {
                    _logger.LogWarning($"Intento de crear usuario con correo duplicado: {usuarioDto.Correo}");
                    return BadRequest("El correo electrónico ya está registrado.");
                }

                var usuario = new Usuario
                {
                    Nombre = usuarioDto.Nombre,
                    Apellido = usuarioDto.Apellido,
                    Correo = usuarioDto.Correo,
                    //FechaNacimiento= usuarioDto.FechaNacimiento,
                    //edad = CalcularEdad(usuarioDto.FechaNacimiento),
                    Imagen = usuarioDto.Imagen,
                    Contrasena = usuarioDto.Contrasena,
                    Telefono = usuarioDto.Telefono,
                    EstadoId = usuarioDto.EstadoId,
                    RolId = usuarioDto.RolId,
                    FechaCreacion = DateTime.UtcNow

                };

                _logger.LogInformation($"Guardando usuario: {usuario.Correo}");
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                try
                {
                    var emailModel = new EmailModel
                    {
                        Email = usuario.Correo,
                        Subject = "Bienvenido a TiendaKeytlin",
                        Message = $"Hola {usuario.Nombre},\n\n" +
                                  $"Tu cuenta ha sido creada exitosamente. " +
                                  $"Tu contraseña es: {usuario.Contrasena}\n\n" +
                                  "Por favor, cambia tu contraseña después de iniciar sesión."
                    };

                    await _emailService.SendEmailAsync(emailModel);
                    _logger.LogInformation($"Correo de bienvenida enviado a: {usuario.Correo}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al enviar correo a {usuario.Correo}: {ex.Message}");
                }

                var usuarioResponse = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Correo,
                    usuario.FechaNacimiento,
                    usuario.edad,
                    usuario.Imagen,
                    usuario.Telefono,
                    usuario.FechaCreacion,
                    usuario.Estado,
                    usuario.Rol
                };

                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuarioResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear usuario: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al crear el usuario");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UsuarioUpdateDto usuarioDto)
        {
            try
            {
                if (id != usuarioDto.Id) 
                {
                    _logger.LogWarning($"ID no coincidente en actualización: {id} vs {usuarioDto.Id}");
                    return BadRequest("El ID no coincide con el usuario a actualizar");
                }

                var usuarioExistente = await _context.Usuarios.FindAsync(id);
                if (usuarioExistente == null)
                {
                    _logger.LogWarning($"Intento de actualizar usuario inexistente: {id}");
                    return NotFound($"No se encontró el usuario con ID {id}");
                }

                usuarioExistente.Nombre = usuarioDto.Nombre;
                usuarioExistente.Apellido = usuarioDto.Apellido;
                usuarioExistente.Correo = usuarioDto.Correo;
                usuarioExistente.Imagen = usuarioDto.Imagen;
                usuarioExistente.FechaNacimiento = usuarioDto.FechaNacimiento.ToUniversalTime();
                usuarioExistente.edad = CalcularEdad(usuarioDto.FechaNacimiento); // <-- Recalcular edad
                usuarioExistente.Telefono = usuarioDto.Telefono;
                usuarioExistente.EstadoId = usuarioDto.EstadoId;
                usuarioExistente.RolId = usuarioDto.RolId;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Usuario actualizado correctamente: {id}");

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError($"Error de concurrencia al actualizar usuario {id}: {ex.Message}");
                return StatusCode(409, "Error de concurrencia al actualizar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar usuario {id}: {ex.Message}");
                return StatusCode(500, "Error interno al actualizar el usuario");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning($"Intento de eliminar usuario inexistente: {id}");
                    return NotFound($"No se encontró el usuario con ID {id}");
                }

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario eliminado correctamente: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar usuario {id}: {ex.Message}");
                return StatusCode(500, "Error interno al eliminar el usuario");
            }
        }

        [HttpPut("eliminar-logico/{id}")]
        public async Task<IActionResult> EliminarLogicoUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    _logger.LogWarning($"Intento de eliminar lógicamente un usuario inexistente: {id}");
                    return NotFound($"No se encontró el usuario con ID {id}");
                }

                usuario.EstadoId = 3; // Estado 3 = Eliminado lógicamente

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario eliminado lógicamente (EstadoId = 3): {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar lógicamente el usuario {id}: {ex.Message}");
                return StatusCode(500, "Error interno al eliminar lógicamente el usuario");
            }
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
