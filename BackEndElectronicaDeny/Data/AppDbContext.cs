using BackEnd_ElectronicaDeny.Models;
using BackEndElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_ElectronicaDeny.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<AperturaCaja> AperturasCaja { get; set; }
        public DbSet<CierreCaja> CierresCaja { get; set; }
        public DbSet<ClasificacionCaja> ClasificacionesCaja { get; set; }
        public DbSet<SaldosCaja> SaldosCaja { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RolUsuario> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Stock> Stock { get; set; } = null!;
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }
        public DbSet<Clientes> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== AperturaCaja =====
            modelBuilder.Entity<AperturaCaja>(e =>
            {
                e.ToTable("AperturasCaja");
                e.Property(x => x.FechaAperturaUtc)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                e.Property(x => x.MontoApertura).HasColumnType("decimal(18,2)");
                e.Property(x => x.CajeroNombre).HasMaxLength(120);
                e.Property(x => x.Notas).HasMaxLength(500);
            });

            // ===== CierreCaja =====
            modelBuilder.Entity<CierreCaja>(e =>
            {
                e.ToTable("CierresCaja");
                e.Property(x => x.FechaCierreUtc)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                // Cierre -> Usuario (N:1)
                e.HasOne(x => x.Usuario)
                 .WithMany()
                 .HasForeignKey(x => x.UsuarioId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Cierre -> Saldos (1:1)
                e.HasOne(x => x.Saldos)
                 .WithOne(s => s.CierreCaja)
                 .HasForeignKey<SaldosCaja>(s => s.CierreCajaId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClasificacionCaja>()
                .ToTable("ClasificacionesCaja");

            modelBuilder.Entity<SaldosCaja>()
                .ToTable("SaldosCaja");

            //Empresa
            modelBuilder.Entity<Empresa>().HasData(
                      new Empresa { Id = 1, Nombre = "Electrónica Deny", Telefono = "3883 6490", Correo = "electronicadeny@gmail.com", Direccion = "Aldea Chuiquel Central Uno, Sololá, Sololá" }
                     );

                //Usuario Admin
                modelBuilder.Entity<Usuario>().HasData(
                      new Usuario { Id = 1, Nombre = "Deny", Apellido = "Xoquic",
                          Telefono = "5881 6213", Correo = "electronicadeny@gmail.com",
                          Contrasena = "@Admin2025", Estado = 1, RolId = 1, }
                     );

                // Insertar datos iniciales para Roles
                modelBuilder.Entity<RolUsuario>().HasData(
                    new RolUsuario { Id = 1, Nombre = "Admin" },
                    new RolUsuario { Id = 2, Nombre = "Vendedor" }
                );

                modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

                // Insertar datos iniciales para Permisos
                modelBuilder.Entity<Permiso>().HasData(

                    // Dashboard
                    new Permiso { Id = 1, Nombre = "Ver Dashboard" },
                    new Permiso { Id = 2, Nombre = "Crear Dashboard" },
                    new Permiso { Id = 3, Nombre = "Editar Dashboard" },
                    new Permiso { Id = 4, Nombre = "Eliminar Dashboard" },

                    // Apertura Caja
                    new Permiso { Id = 5, Nombre = "Ver Apertura Caja" },
                    new Permiso { Id = 6, Nombre = "Crear Apertura Caja" },
                    new Permiso { Id = 7, Nombre = "Editar Apertura Caja" },
                    new Permiso { Id = 8, Nombre = "Eliminar Apertura Caja" },

                    // Cierre Caja
                    new Permiso { Id = 9, Nombre = "Ver Cierre Caja" },
                    new Permiso { Id = 10, Nombre = "Crear Cierre Caja" },
                    new Permiso { Id = 11, Nombre = "Editar Cierre Caja" },
                    new Permiso { Id = 12, Nombre = "Eliminar Cierre Caja" },

                    // Productos
                    new Permiso { Id = 13, Nombre = "Ver Productos" },
                    new Permiso { Id = 14, Nombre = "Crear Productos" },
                    new Permiso { Id = 15, Nombre = "Editar Productos" },
                    new Permiso { Id = 16, Nombre = "Eliminar Productos" },

                    // Categorías
                    new Permiso { Id = 17, Nombre = "Ver Categorías" },
                    new Permiso { Id = 18, Nombre = "Crear Categorías" },
                    new Permiso { Id = 19, Nombre = "Editar Categorías" },
                    new Permiso { Id = 20, Nombre = "Eliminar Categorías" },

                    // Proveedores
                    new Permiso { Id = 21, Nombre = "Ver Proveedores" },
                    new Permiso { Id = 22, Nombre = "Crear Proveedores" },
                    new Permiso { Id = 23, Nombre = "Editar Proveedores" },
                    new Permiso { Id = 24, Nombre = "Eliminar Proveedores" },

                    // Inventario / Stock
                    new Permiso { Id = 25, Nombre = "Ver Inventario" },
                    new Permiso { Id = 26, Nombre = "Crear Inventario" },
                    new Permiso { Id = 27, Nombre = "Editar Inventario" },
                    new Permiso { Id = 28, Nombre = "Eliminar Inventario" },

                    // Pedidos
                    new Permiso { Id = 29, Nombre = "Ver Pedidos" },
                    new Permiso { Id = 30, Nombre = "Crear Pedidos" },
                    new Permiso { Id = 31, Nombre = "Editar Pedidos" },
                    new Permiso { Id = 32, Nombre = "Eliminar Pedidos" },

                    // Ventas
                    new Permiso { Id = 33, Nombre = "Ver Ventas" },
                    new Permiso { Id = 34, Nombre = "Crear Ventas" },
                    new Permiso { Id = 35, Nombre = "Editar Ventas" },
                    new Permiso { Id = 36, Nombre = "Eliminar Ventas" },

                    // Historial
                    new Permiso { Id = 37, Nombre = "Ver Historial" },
                    new Permiso { Id = 38, Nombre = "Crear Historial" },
                    new Permiso { Id = 39, Nombre = "Editar Historial" },
                    new Permiso { Id = 40, Nombre = "Eliminar Historial" },

                    // Usuarios
                    new Permiso { Id = 41, Nombre = "Ver Usuarios" },
                    new Permiso { Id = 42, Nombre = "Crear Usuarios" },
                    new Permiso { Id = 43, Nombre = "Editar Usuarios" },
                    new Permiso { Id = 44, Nombre = "Eliminar Usuarios" },

                    // Contacto
                    new Permiso { Id = 45, Nombre = "Ver Contacto" },
                    new Permiso { Id = 46, Nombre = "Crear Contacto" },
                    new Permiso { Id = 47, Nombre = "Editar Contacto" },
                    new Permiso { Id = 48, Nombre = "Eliminar Contacto" },

                    // Roles
                    new Permiso { Id = 49, Nombre = "Ver Roles" },
                    new Permiso { Id = 50, Nombre = "Crear Roles" },
                    new Permiso { Id = 51, Nombre = "Editar Roles" },
                    new Permiso { Id = 52, Nombre = "Eliminar Roles" },

                    // Reportes Usuarios
                    new Permiso { Id = 53, Nombre = "Ver Reportes de Usuarios" },
                    new Permiso { Id = 54, Nombre = "Crear Reportes de Usuarios" },
                    new Permiso { Id = 55, Nombre = "Editar Reportes de Usuarios" },
                    new Permiso { Id = 56, Nombre = "Eliminar Reportes de Usuarios" },

                    // Reportes Ventas
                    new Permiso { Id = 57, Nombre = "Ver Reportes de Ventas" },
                    new Permiso { Id = 58, Nombre = "Crear Reportes de Ventas" },
                    new Permiso { Id = 59, Nombre = "Editar Reportes de Ventas" },
                    new Permiso { Id = 60, Nombre = "Eliminar Reportes de Ventas" },

                    // Reportes Pedidos
                    new Permiso { Id = 61, Nombre = "Ver Reportes de Pedidos" },
                    new Permiso { Id = 62, Nombre = "Crear Reportes de Pedidos" },
                    new Permiso { Id = 63, Nombre = "Editar Reportes de Pedidos" },
                    new Permiso { Id = 64, Nombre = "Eliminar Reportes de Pedidos" },

                    // Reportes Inventario
                    new Permiso { Id = 65, Nombre = "Ver Reportes de Inventario" },
                    new Permiso { Id = 66, Nombre = "Crear Reportes de Inventario" },
                    new Permiso { Id = 67, Nombre = "Editar Reportes de Inventario" },
                    new Permiso { Id = 68, Nombre = "Eliminar Reportes de Inventario" },

                    // Clientes (nuevos)
                    new Permiso { Id = 69, Nombre = "Ver Clientes" },
                    new Permiso { Id = 70, Nombre = "Crear Clientes" },
                    new Permiso { Id = 71, Nombre = "Editar Clientes" },
                    new Permiso { Id = 72, Nombre = "Eliminar Clientes" }


                );

            modelBuilder.Entity<RolPermiso>(e =>
            {
                e.ToTable("RolPermisos");
                e.HasKey(x => new { x.RolId, x.PermisoId });

                e.HasOne(x => x.Rol)
                    .WithMany(r => r.RolPermisos)
                    .HasForeignKey(x => x.RolId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Permiso)
                    .WithMany(p => p.RolPermisos)
                    .HasForeignKey(x => x.PermisoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            int[] permisosAdmin =
            {
                1,2,3,4,
                5,6,7,8,
                9,10,11,12,
                13,14,15,16,
                17,18,19,20,
                21,22,23,24,
                25,26,27,28,               
                29,30,31,32,
                33,34,35,36,
                37,38,39,40,
                41,42,43,44,
                45,46,47,48,
                49,50,51,52,
                53,54,55,56,
                57,58,59,60,
                61,62,63,64,
                65,66,67,68,
                69,70,71,72
            };

            var rpSeed = Enumerable.Range(1, 72)
                .Select(pid => new RolPermiso { RolId = 1, PermisoId = pid });
            modelBuilder.Entity<RolPermiso>().HasData(rpSeed);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Proveedor>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Productos>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Productos>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Proveedor)
                .WithMany()
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>(b =>
            {
                b.ToTable("Stocks");
                b.HasKey(s => s.ProductoId);
                b.Property(s => s.PrecioCompra).HasColumnType("decimal(18,2)");
                b.Property(s => s.PrecioVenta).HasColumnType("decimal(18,2)");
                b.Property(s => s.NombreCategoria).HasMaxLength(120);
                b.Property(s => s.Imagen).HasMaxLength(500);
                b.HasOne(s => s.Producto)
                 .WithOne(p => p.Stock)
                 .HasForeignKey<Stock>(s => s.ProductoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Productos>(b =>
            {
                b.Property(p => p.PrecioVenta).HasColumnType("decimal(18,2)");
                b.Property(p => p.PrecioAdquisicion).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Clientes>(e =>
            {
                e.HasIndex(c => c.Telefono).IsUnique();
                e.Property(c => c.FechaRegistro)
                 .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            });

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Vendedor)
                .WithMany()
                .HasForeignKey(v => v.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}