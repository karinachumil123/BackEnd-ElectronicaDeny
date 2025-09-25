using BackEnd_ElectronicaDeny.Models;
using BackEndElectronicaDeny.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_ElectronicaDeny.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Estados> Estados { get; set; }
        public DbSet<RolUsuario> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<Productos> Productos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Insertar datos iniciales para Estados  
            modelBuilder.Entity<Estados>().HasData(
                new Estados { Id = 1, Nombre = "Activo" },
                new Estados { Id = 2, Nombre = "Inactivo" },
                new Estados { Id = 3, Nombre = "Eliminado" }
            );

            //Empresa
            modelBuilder.Entity<Empresa>().HasData(
                  new Empresa { Id = 1, Nombre = "Electrónica Deny", Telefono = "3883 6490", Correo = "electronicadeny@gmail.com", Direccion = "Aldea Chuiquel Central Uno, Sololá, Sololá" }
                 );

            //Usuario Admin
            modelBuilder.Entity<Usuario>().HasData(
                  new Usuario { Id = 1, Nombre = "Deny", Apellido = "Xoquic", Telefono = "5881 6213", Correo = "electronicadeny@gmail.com", Contrasena = "@Admin2025", EstadoId = 1, RolId = 1, }
                 );

            // Insertar datos iniciales para Roles
            modelBuilder.Entity<RolUsuario>().HasData(
                new RolUsuario { Id = 1, Nombre = "Admin" },
                new RolUsuario { Id = 2, Nombre = "Vendedor" }
            );

            modelBuilder.Entity<RolPermiso>()
            .HasKey(rp => new { rp.RolId, rp.PermisoId });

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
                new Permiso { Id = 68, Nombre = "Eliminar Reportes de Inventario" }

            );

            // Asignar todos los permisos al rol Admin
            modelBuilder.Entity<RolPermiso>().HasData(
                new RolPermiso { Id = 1, RolId = 1, PermisoId = 1 },
                new RolPermiso { Id = 2, RolId = 1, PermisoId = 2 },
                new RolPermiso { Id = 3, RolId = 1, PermisoId = 3 },
                new RolPermiso { Id = 4, RolId = 1, PermisoId = 4 },
                new RolPermiso { Id = 5, RolId = 1, PermisoId = 5 },
                new RolPermiso { Id = 6, RolId = 1, PermisoId = 6 },
                new RolPermiso { Id = 7, RolId = 1, PermisoId = 7 },
                new RolPermiso { Id = 8, RolId = 1, PermisoId = 8 },
                new RolPermiso { Id = 9, RolId = 1, PermisoId = 9 },
                new RolPermiso { Id = 10, RolId = 1, PermisoId = 10 },
                new RolPermiso { Id = 11, RolId = 1, PermisoId = 11 },
                new RolPermiso { Id = 12, RolId = 1, PermisoId = 12 },
                new RolPermiso { Id = 13, RolId = 1, PermisoId = 13 },
                new RolPermiso { Id = 14, RolId = 1, PermisoId = 14 },
                new RolPermiso { Id = 15, RolId = 1, PermisoId = 15 },
                new RolPermiso { Id = 16, RolId = 1, PermisoId = 16 },
                new RolPermiso { Id = 17, RolId = 1, PermisoId = 17 },
                new RolPermiso { Id = 18, RolId = 1, PermisoId = 18 },
                new RolPermiso { Id = 19, RolId = 1, PermisoId = 19 },
                new RolPermiso { Id = 20, RolId = 1, PermisoId = 20 },
                new RolPermiso { Id = 21, RolId = 1, PermisoId = 21 },
                new RolPermiso { Id = 22, RolId = 1, PermisoId = 22 },
                new RolPermiso { Id = 23, RolId = 1, PermisoId = 23 },
                new RolPermiso { Id = 24, RolId = 1, PermisoId = 24 },
                new RolPermiso { Id = 25, RolId = 1, PermisoId = 25 },
                new RolPermiso { Id = 26, RolId = 1, PermisoId = 26 },
                new RolPermiso { Id = 27, RolId = 1, PermisoId = 27 },
                new RolPermiso { Id = 28, RolId = 1, PermisoId = 28 },
                new RolPermiso { Id = 29, RolId = 1, PermisoId = 29 },
                new RolPermiso { Id = 30, RolId = 1, PermisoId = 30 },
                new RolPermiso { Id = 31, RolId = 1, PermisoId = 31 },
                new RolPermiso { Id = 32, RolId = 1, PermisoId = 32 },
                new RolPermiso { Id = 33, RolId = 1, PermisoId = 33 },
                new RolPermiso { Id = 34, RolId = 1, PermisoId = 34 },
                new RolPermiso { Id = 35, RolId = 1, PermisoId = 35 },
                new RolPermiso { Id = 36, RolId = 1, PermisoId = 36 },
                new RolPermiso { Id = 37, RolId = 1, PermisoId = 37 },
                new RolPermiso { Id = 38, RolId = 1, PermisoId = 38 },
                new RolPermiso { Id = 39, RolId = 1, PermisoId = 39 },
                new RolPermiso { Id = 40, RolId = 1, PermisoId = 40 },
                new RolPermiso { Id = 41, RolId = 1, PermisoId = 41 },
                new RolPermiso { Id = 42, RolId = 1, PermisoId = 42 },
                new RolPermiso { Id = 43, RolId = 1, PermisoId = 43 },
                new RolPermiso { Id = 44, RolId = 1, PermisoId = 44 },
                new RolPermiso { Id = 45, RolId = 1, PermisoId = 45 },
                new RolPermiso { Id = 46, RolId = 1, PermisoId = 46 },
                new RolPermiso { Id = 47, RolId = 1, PermisoId = 47 },
                new RolPermiso { Id = 48, RolId = 1, PermisoId = 48 },
                new RolPermiso { Id = 49, RolId = 1, PermisoId = 49 },
                new RolPermiso { Id = 50, RolId = 1, PermisoId = 50 },
                new RolPermiso { Id = 51, RolId = 1, PermisoId = 51 },
                new RolPermiso { Id = 52, RolId = 1, PermisoId = 52 },
                new RolPermiso { Id = 53, RolId = 1, PermisoId = 53 },
                new RolPermiso { Id = 54, RolId = 1, PermisoId = 54 },
                new RolPermiso { Id = 55, RolId = 1, PermisoId = 55 },
                new RolPermiso { Id = 56, RolId = 1, PermisoId = 56 },
                new RolPermiso { Id = 57, RolId = 1, PermisoId = 57 },
                new RolPermiso { Id = 58, RolId = 1, PermisoId = 58 },
                new RolPermiso { Id = 59, RolId = 1, PermisoId = 59 },
                new RolPermiso { Id = 60, RolId = 1, PermisoId = 60 },
                new RolPermiso { Id = 61, RolId = 1, PermisoId = 61 },
                new RolPermiso { Id = 62, RolId = 1, PermisoId = 62 },
                new RolPermiso { Id = 63, RolId = 1, PermisoId = 63 },
                new RolPermiso { Id = 64, RolId = 1, PermisoId = 64 },
                new RolPermiso { Id = 65, RolId = 1, PermisoId = 65 },
                new RolPermiso { Id = 66, RolId = 1, PermisoId = 66 },
                new RolPermiso { Id = 67, RolId = 1, PermisoId = 67 },
                new RolPermiso { Id = 68, RolId = 1, PermisoId = 68 }
            );


            // Configurar las relaciones
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Estado)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EstadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurar relación entre RolUsuario y Permiso 
            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)  // Relación con RolUsuario
                .WithMany()  // RolUsuario no tiene una colección de Permisos, ya que se maneja a través de RolPermiso
                .HasForeignKey(rp => rp.RolId);  // Clave foránea en RolPermiso para RolUsuario

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)  // Relación con Permiso
                .WithMany()  // Permiso no tiene una colección de RolPermiso, ya que se maneja a través de RolPermiso
                .HasForeignKey(rp => rp.PermisoId);  // Clave foránea en RolPermiso para Permiso

            //Relaciones Proveedor
            modelBuilder.Entity<Proveedor>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();  // Muy importante

            // Producto - Categoria (muchos-a-uno)
            modelBuilder.Entity<Productos>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto - Proveedor (muchos-a-uno)
            modelBuilder.Entity<Productos>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación: Pedido 1 - * DetallePedido
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade); // al eliminar Pedido, se eliminan los Detalles

            // Relación: DetallePedido - Producto (muchos a uno)
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany() // si tienes navegación inversa, cámbiala por .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict); // no borrar Producto si hay detalles

            // Relación: Pedido - Proveedor
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Proveedor)
                .WithMany() // si hay navegación inversa, cámbiala
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            
        }
    }
}
