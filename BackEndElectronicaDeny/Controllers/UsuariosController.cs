using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
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
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuarioController(
            AppDbContext context,
            EmailService emailService,
            ILogger<UsuarioController> logger,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        private static string EstadoNombre(int estado) =>
            estado == 1 ? "Activo" :
            estado == 2 ? "Inactivo" : $"Desconocido({estado})";

        // Ahora acepta DateTime? para evitar conversiones de nullables
        private int CalcularEdad(DateTime? fechaNacimiento)
        {
            if (!fechaNacimiento.HasValue) return 0;
            var fn = fechaNacimiento.Value.Date;
            var hoy = DateTime.Today;
            var edad = hoy.Year - fn.Year;
            if (fn > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        // GET: api/usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Estado == 1 || u.Estado == 2)
                    .Include(u => u.Rol)
                    .AsNoTracking()
                    .ToListAsync();

                var usuariosResponse = usuarios.Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Correo,
                    u.Imagen,
                    u.FechaNacimiento,
                    edad = CalcularEdad(u.FechaNacimiento),
                    u.Telefono,
                    u.FechaCreacion,
                    u.UltimoInicioSesion,
                    Estado = u.Estado,
                    EstadoNombre = EstadoNombre(u.Estado),
                    Rol = u.Rol != null ? u.Rol.Nombre : "(sin rol)",
                    u.RolId
                }).ToList();

                return Ok(usuariosResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, "Error interno al obtener la lista de usuarios");
            }
        }

        // GET: api/usuario/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetUsuario(int id)
        {
            try
            {
                var u = await _context.Usuarios
                    .Include(x => x.Rol)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (u == null)
                    return NotFound($"No se encontró el usuario con ID {id}");

                var dto = new
                {
                    u.Id,
                    u.Nombre,
                    u.Apellido,
                    u.Correo,
                    u.Telefono,
                    u.Imagen,
                    u.FechaNacimiento,
                    edad = CalcularEdad(u.FechaNacimiento),
                    u.FechaCreacion,
                    u.UltimoInicioSesion,
                    Estado = u.Estado,
                    EstadoNombre = EstadoNombre(u.Estado),
                    u.RolId,
                    RolNombre = u.Rol != null ? u.Rol.Nombre : "(sin rol)"
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {Id}", id);
                return StatusCode(500, "Error interno al obtener el usuario");
            }
        }

        // GET: api/usuario/reporte/pdf
        [HttpGet("reporte/pdf")]
        public async Task<IActionResult> DescargarReportePdf()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Estado == 1 || u.Estado == 2)
                    .Include(u => u.Rol)
                    .AsNoTracking()
                    .ToListAsync();

                var stream = new MemoryStream();

                using (var writer = new iText.Kernel.Pdf.PdfWriter(stream))
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                using (var document = new iText.Layout.Document(pdf))
                {
                    PdfFont font = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    PdfFont boldFont = iText.Kernel.Font.PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    document.Add(new iText.Layout.Element.Paragraph("Reporte de Usuarios")
                        .SetFont(boldFont)
                        .SetFontSize(18)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                    document.Add(new iText.Layout.Element.Paragraph("\n"));

                    var table = new iText.Layout.Element.Table(8).UseAllAvailableWidth();

                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("ID").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Nombre").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Apellido").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Email").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Teléfono").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Estado").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Rol").SetFont(boldFont)));
                    table.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Creación").SetFont(boldFont)));

                    foreach (var u in usuarios)
                    {
                        table.AddCell(u.Id.ToString());
                        table.AddCell(u.Nombre ?? "-");
                        table.AddCell(u.Apellido ?? "-");
                        table.AddCell(u.Correo ?? "-");
                        table.AddCell(u.Telefono ?? "-");
                        table.AddCell(EstadoNombre(u.Estado));
                        table.AddCell(u.Rol?.Nombre ?? "-");
                        table.AddCell(u.FechaCreacion?.ToString("yyyy-MM-dd HH:mm") ?? "-");
                    }

                    document.Add(table);
                    document.Close();
                }

                return File(stream.ToArray(), "application/pdf", "Reporte_Usuarios.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF");
                return StatusCode(500, "Error interno al generar el PDF: " + ex.Message);
            }
        }

        // POST: api/usuario
        [HttpPost]
        public async Task<ActionResult<object>> PostUsuario([FromBody] UsuarioCreateDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload vacío.");

                string correoNorm = (dto.Correo ?? "").Trim().ToLowerInvariant();
                string telNorm = (dto.Telefono ?? "").Trim();

                if (string.IsNullOrWhiteSpace(correoNorm)) return BadRequest("El correo electrónico es obligatorio.");
                if (string.IsNullOrWhiteSpace(dto.Contrasena)) return BadRequest("La contraseña es obligatoria.");
                if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Apellido))
                    return BadRequest("El nombre y apellido son obligatorios.");
                if (dto.Estado != 1 && dto.Estado != 2)
                    return BadRequest("Estado inválido. Use 1=Activo, 2=Inactivo.");
                if (dto.RolId <= 0)
                    return BadRequest("RolId inválido.");

                // Verifica que el rol exista
                bool rolExiste = await _context.Roles.AsNoTracking().AnyAsync(r => r.Id == dto.RolId);
                if (!rolExiste)
                    return BadRequest($"El RolId {dto.RolId} no existe.");

                // Duplicados normalizados
                bool correoDuplicado = await _context.Usuarios.AsNoTracking()
                    .AnyAsync(u => (u.Correo ?? "").Trim().ToLower() == correoNorm);
                bool telDuplicado = !string.IsNullOrEmpty(telNorm) && await _context.Usuarios.AsNoTracking()
                    .AnyAsync(u => (u.Telefono ?? "").Trim() == telNorm);

                if (correoDuplicado || telDuplicado)
                {
                    var msg = correoDuplicado && telDuplicado
                        ? "No se pudo crear: el correo y el teléfono ya están registrados."
                        : (correoDuplicado ? "No se pudo crear: el correo ya está registrado."
                                           : "No se pudo crear: el teléfono ya está registrado.");
                    return Conflict(msg);
                }

                var usuario = new Usuario
                {
                    Nombre = dto.Nombre?.Trim(),
                    Apellido = dto.Apellido?.Trim(),
                    Correo = correoNorm,
                    Telefono = string.IsNullOrEmpty(telNorm) ? null : telNorm,
                    Imagen = dto.Imagen,
                    // Si la propiedad de la entidad es tipo "date" en base, usa .Date para truncar la hora
                    FechaNacimiento = dto.FechaNacimiento?.Date,
                    edad = CalcularEdad(dto.FechaNacimiento),
                    Estado = dto.Estado,
                    RolId = dto.RolId,
                    FechaCreacion = DateTime.UtcNow
                };

                var plain = dto.Contrasena!;
                usuario.Contrasena = _passwordHasher.HashPassword(usuario, plain);

                _context.Usuarios.Add(usuario);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    var detalle = ex.InnerException?.Message ?? ex.Message;
                    return BadRequest($"Error de base de datos al crear el usuario: {detalle}");
                }

                // Envío de correo (no interrumpe)
                try
                {
                    var emailModel = new EmailModel
                    {
                        Email = usuario.Correo,
                        Subject = "Bienvenido a Electrónica Deny",
                        Message = $"Hola {usuario.Nombre},\n\nTu cuenta fue creada.\nTu contraseña es: {plain}\n\nCámbiala al iniciar sesión."
                    };
                    await _emailService.SendEmailAsync(emailModel);
                }
                catch (Exception mailEx)
                {
                    _logger.LogError(mailEx, "Error al enviar correo a {Correo}", usuario.Correo);
                }

                var resp = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Correo,
                    usuario.FechaNacimiento,
                    edad = CalcularEdad(usuario.FechaNacimiento),
                    usuario.Imagen,
                    usuario.Telefono,
                    usuario.FechaCreacion,
                    Estado = usuario.Estado,
                    EstadoNombre = EstadoNombre(usuario.Estado),
                    usuario.RolId
                };

                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al crear usuario");
                return StatusCode(500, "Error interno del servidor al crear el usuario");
            }
        }

        // PUT: api/usuario/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UsuarioUpdateDto dto)
        {
            try
            {
                if (dto == null) return BadRequest("Payload vacío.");
                if (id != dto.Id) return BadRequest("El ID no coincide con el usuario a actualizar");
                if (dto.Estado != 1 && dto.Estado != 2) return BadRequest("Estado inválido. Use 1=Activo, 2=Inactivo.");

                var entity = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) return NotFound($"No se encontró el usuario con ID {id}");

                entity.Nombre = dto.Nombre?.Trim();
                entity.Apellido = dto.Apellido?.Trim();
                entity.Correo = dto.Correo?.Trim();
                entity.Imagen = dto.Imagen;
                entity.Telefono = dto.Telefono?.Trim();
                entity.FechaNacimiento = dto.FechaNacimiento?.Date;
                entity.edad = CalcularEdad(dto.FechaNacimiento);
                entity.Estado = dto.Estado;      // 1/2
                entity.RolId = dto.RolId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuario actualizado correctamente: {Id}", id);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al actualizar usuario {Id}", id);
                return StatusCode(409, "Error de concurrencia al actualizar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
                return StatusCode(500, "Error interno al actualizar el usuario");
            }
        }

        // DELETE físico
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return NotFound($"No se encontró el usuario con ID {id}");

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario eliminado correctamente: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
                return StatusCode(500, "Error interno al eliminar el usuario");
            }
        }

        // Eliminación lógica -> Estado = 2
        [HttpPut("eliminar-logico/{id:int}")]
        public async Task<IActionResult> EliminarLogicoUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null) return NotFound($"No se encontró el usuario con ID {id}");

                usuario.Estado = 2;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuario marcado como Inactivo (Estado = 2): {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lógicamente el usuario {Id}", id);
                return StatusCode(500, "Error interno al eliminar lógicamente el usuario");
            }
        }
    }
}
