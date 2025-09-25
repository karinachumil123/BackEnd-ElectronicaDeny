using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackEndElectronicaDeny.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoriaNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstadoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    NombreContacto = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    TelefonoContacto = table.Column<string>(type: "text", nullable: true),
                    Correo = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    EstadoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroPedido = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NombrePedido = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProveedorId = table.Column<int>(type: "integer", nullable: false),
                    EstadoPedidoId = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TotalPedido = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    CodigoProducto = table.Column<string>(type: "text", nullable: false),
                    MarcaProducto = table.Column<string>(type: "text", nullable: false),
                    PrecioAdquisicion = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric", nullable: false),
                    EstadoId = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    Imagen = table.Column<string>(type: "text", nullable: true),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    ProveedorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    RolUsuarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permisos_Roles_RolUsuarioId",
                        column: x => x.RolUsuarioId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    Correo = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Contrasena = table.Column<string>(type: "text", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    edad = table.Column<int>(type: "integer", nullable: false),
                    Imagen = table.Column<string>(type: "text", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimoInicioSesion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstadoId = table.Column<int>(type: "integer", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    CodigoRecuperacion = table.Column<string>(type: "text", nullable: true),
                    FechaExpiracionCodigo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Estados_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "Estados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallePedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProductosId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallePedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallePedidos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallePedidos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallePedidos_Productos_ProductosId",
                        column: x => x.ProductosId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    PermisoId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Empresa",
                columns: new[] { "Id", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[] { 1, "electronicadeny@gmail.com", "Aldea Chuiquel Central Uno, Sololá, Sololá", "Electrónica Deny", "3883 6490" });

            migrationBuilder.InsertData(
                table: "Estados",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Activo" },
                    { 2, "Inactivo" },
                    { 3, "Eliminado" }
                });

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "Id", "Nombre", "RolUsuarioId" },
                values: new object[,]
                {
                    { 1, "Ver Dashboard", null },
                    { 2, "Crear Dashboard", null },
                    { 3, "Editar Dashboard", null },
                    { 4, "Eliminar Dashboard", null },
                    { 5, "Ver Apertura Caja", null },
                    { 6, "Crear Apertura Caja", null },
                    { 7, "Editar Apertura Caja", null },
                    { 8, "Eliminar Apertura Caja", null },
                    { 9, "Ver Cierre Caja", null },
                    { 10, "Crear Cierre Caja", null },
                    { 11, "Editar Cierre Caja", null },
                    { 12, "Eliminar Cierre Caja", null },
                    { 13, "Ver Productos", null },
                    { 14, "Crear Productos", null },
                    { 15, "Editar Productos", null },
                    { 16, "Eliminar Productos", null },
                    { 17, "Ver Categorías", null },
                    { 18, "Crear Categorías", null },
                    { 19, "Editar Categorías", null },
                    { 20, "Eliminar Categorías", null },
                    { 21, "Ver Proveedores", null },
                    { 22, "Crear Proveedores", null },
                    { 23, "Editar Proveedores", null },
                    { 24, "Eliminar Proveedores", null },
                    { 25, "Ver Inventario", null },
                    { 26, "Crear Inventario", null },
                    { 27, "Editar Inventario", null },
                    { 28, "Eliminar Inventario", null },
                    { 29, "Ver Pedidos", null },
                    { 30, "Crear Pedidos", null },
                    { 31, "Editar Pedidos", null },
                    { 32, "Eliminar Pedidos", null },
                    { 33, "Ver Ventas", null },
                    { 34, "Crear Ventas", null },
                    { 35, "Editar Ventas", null },
                    { 36, "Eliminar Ventas", null },
                    { 37, "Ver Historial", null },
                    { 38, "Crear Historial", null },
                    { 39, "Editar Historial", null },
                    { 40, "Eliminar Historial", null },
                    { 41, "Ver Usuarios", null },
                    { 42, "Crear Usuarios", null },
                    { 43, "Editar Usuarios", null },
                    { 44, "Eliminar Usuarios", null },
                    { 45, "Ver Contacto", null },
                    { 46, "Crear Contacto", null },
                    { 47, "Editar Contacto", null },
                    { 48, "Eliminar Contacto", null },
                    { 49, "Ver Roles", null },
                    { 50, "Crear Roles", null },
                    { 51, "Editar Roles", null },
                    { 52, "Eliminar Roles", null },
                    { 53, "Ver Reportes de Usuarios", null },
                    { 54, "Crear Reportes de Usuarios", null },
                    { 55, "Editar Reportes de Usuarios", null },
                    { 56, "Eliminar Reportes de Usuarios", null },
                    { 57, "Ver Reportes de Ventas", null },
                    { 58, "Crear Reportes de Ventas", null },
                    { 59, "Editar Reportes de Ventas", null },
                    { 60, "Eliminar Reportes de Ventas", null },
                    { 61, "Ver Reportes de Pedidos", null },
                    { 62, "Crear Reportes de Pedidos", null },
                    { 63, "Editar Reportes de Pedidos", null },
                    { 64, "Eliminar Reportes de Pedidos", null },
                    { 65, "Ver Reportes de Inventario", null },
                    { 66, "Crear Reportes de Inventario", null },
                    { 67, "Editar Reportes de Inventario", null },
                    { 68, "Eliminar Reportes de Inventario", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Vendedor" }
                });

            migrationBuilder.InsertData(
                table: "RolPermisos",
                columns: new[] { "PermisoId", "RolId", "Id" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 },
                    { 3, 1, 3 },
                    { 4, 1, 4 },
                    { 5, 1, 5 },
                    { 6, 1, 6 },
                    { 7, 1, 7 },
                    { 8, 1, 8 },
                    { 9, 1, 9 },
                    { 10, 1, 10 },
                    { 11, 1, 11 },
                    { 12, 1, 12 },
                    { 13, 1, 13 },
                    { 14, 1, 14 },
                    { 15, 1, 15 },
                    { 16, 1, 16 },
                    { 17, 1, 17 },
                    { 18, 1, 18 },
                    { 19, 1, 19 },
                    { 20, 1, 20 },
                    { 21, 1, 21 },
                    { 22, 1, 22 },
                    { 23, 1, 23 },
                    { 24, 1, 24 },
                    { 25, 1, 25 },
                    { 26, 1, 26 },
                    { 27, 1, 27 },
                    { 28, 1, 28 },
                    { 29, 1, 29 },
                    { 30, 1, 30 },
                    { 31, 1, 31 },
                    { 32, 1, 32 },
                    { 33, 1, 33 },
                    { 34, 1, 34 },
                    { 35, 1, 35 },
                    { 36, 1, 36 },
                    { 37, 1, 37 },
                    { 38, 1, 38 },
                    { 39, 1, 39 },
                    { 40, 1, 40 },
                    { 41, 1, 41 },
                    { 42, 1, 42 },
                    { 43, 1, 43 },
                    { 44, 1, 44 },
                    { 45, 1, 45 },
                    { 46, 1, 46 },
                    { 47, 1, 47 },
                    { 48, 1, 48 },
                    { 49, 1, 49 },
                    { 50, 1, 50 },
                    { 51, 1, 51 },
                    { 52, 1, 52 },
                    { 53, 1, 53 },
                    { 54, 1, 54 },
                    { 55, 1, 55 },
                    { 56, 1, 56 },
                    { 57, 1, 57 },
                    { 58, 1, 58 },
                    { 59, 1, 59 },
                    { 60, 1, 60 },
                    { 61, 1, 61 },
                    { 62, 1, 62 },
                    { 63, 1, 63 },
                    { 64, 1, 64 },
                    { 65, 1, 65 },
                    { 66, 1, 66 },
                    { 67, 1, 67 },
                    { 68, 1, 68 }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Apellido", "CodigoRecuperacion", "Contrasena", "Correo", "EstadoId", "FechaCreacion", "FechaExpiracionCodigo", "FechaNacimiento", "Imagen", "Nombre", "RolId", "Telefono", "UltimoInicioSesion", "edad" },
                values: new object[] { 1, "Xoquic", null, "@Admin2025", "electronicadeny@gmail.com", 1, null, null, null, null, "Deny", 1, "5881 6213", null, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_PedidoId",
                table: "DetallePedidos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_ProductoId",
                table: "DetallePedidos",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallePedidos_ProductosId",
                table: "DetallePedidos",
                column: "ProductosId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_NumeroPedido",
                table: "Pedidos",
                column: "NumeroPedido",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ProveedorId",
                table: "Pedidos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_RolUsuarioId",
                table: "Permisos",
                column: "RolUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ProveedorId",
                table: "Productos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoId",
                table: "RolPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EstadoId",
                table: "Usuarios",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallePedidos");

            migrationBuilder.DropTable(
                name: "Empresa");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Estados");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
