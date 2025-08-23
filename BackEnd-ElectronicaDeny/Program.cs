using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexi�n
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Solo para pruebas de migraci�n / creaci�n de BD
app.Run();
