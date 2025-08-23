using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Solo para pruebas de migración / creación de BD
app.Run();
