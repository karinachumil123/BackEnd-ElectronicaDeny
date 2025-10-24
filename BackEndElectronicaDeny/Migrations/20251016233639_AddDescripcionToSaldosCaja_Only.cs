using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndElectronicaDeny.Migrations
{
    /// <inheritdoc />
    public partial class AddDescripcionToSaldosCaja_Only : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "SaldosCaja",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "SaldosCaja");
        }

    }
}
