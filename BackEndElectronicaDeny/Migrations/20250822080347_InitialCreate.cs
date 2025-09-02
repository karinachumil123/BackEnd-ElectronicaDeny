using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BackEnd_ElectronicaDeny.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { 56, "Eliminar Reportes de Usuarios", null }
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
                table: "Usuarios",
                columns: new[] { "Id", "Apellido", "CodigoRecuperacion", "Contrasena", "Correo", "EstadoId", "FechaCreacion", "FechaExpiracionCodigo", "Imagen", "Nombre", "RolId", "Telefono", "UltimoInicioSesion" },
                values: new object[] { 1, "Chumil", null, "@Admin2025", "tiendakeytelin@gmail.com", 1, null, null, null, "Edwin", 1, "5881 6213", null });

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_RolUsuarioId",
                table: "Permisos",
                column: "RolUsuarioId");

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
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Estados");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
