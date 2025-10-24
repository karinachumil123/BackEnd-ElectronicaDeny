using Microsoft.EntityFrameworkCore;
using BackEnd_ElectronicaDeny.Data;
using BackEnd_ElectronicaDeny.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Identity;
using BackEnd_ElectronicaDeny.Models;
using System.Reflection.Emit;

var builder = WebApplication.CreateBuilder(args);

// ===== Logging =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ===== Services / DI =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<BackEndElectronicaDeny.Services.InventoryService>(); 

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine($"[DB] ConnectionString: {connectionString}");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    // (OPCIONAL) más diagnósticos en Development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Auth JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// Email
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddTransient<EmailService>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Esto aplica cualquier migración pendiente a la MISMA BD de tu connectionString
    db.Database.Migrate();
}

//Usuario Admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var toHash = db.Usuarios
        .Where(u => string.IsNullOrEmpty(u.Contrasena) || !u.Contrasena.StartsWith("AQAAAA"))
        .ToList();

    if (toHash.Count > 0)
    {
        foreach (var u in toHash)
        {
            // si por alguna razón estuviera null/vacía, decide una clave temporal
            if (string.IsNullOrWhiteSpace(u.Contrasena))
                u.Contrasena = "@Temp2025!";

            u.Contrasena = hasher.HashPassword(u, u.Contrasena);
        }
        db.SaveChanges();
        logger.LogInformation("Hasheadas {Count} contraseñas en runtime.", toHash.Count);
    }
    else
    {
        logger.LogDebug("No hay contraseñas en texto plano. Nada que hashear.");
    }
}

// ===== Static files (wwwroot + /uploads) =====

// sirve wwwroot (index.html, assets, etc.)
app.UseDefaultFiles();
app.UseStaticFiles();

// asegura carpeta wwwroot/uploads y expónla en /uploads
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (opcional) redirección https si la usas
// app.UseHttpsRedirection();

// Orden recomendado: CORS -> Auth -> AuthZ -> Controllers
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Si este backend sirve también tu SPA, deja esto;
// si Angular corre en 4200 y solo usas API aquí, puedes quitarlo sin problema.
app.MapFallbackToFile("/index.html");

app.Run();
