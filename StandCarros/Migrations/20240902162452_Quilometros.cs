using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StandCarros.Migrations
{
    /// <inheritdoc />
    public partial class Quilometros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Kilometros",
                table: "Car",
                newName: "Quilometros");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quilometros",
                table: "Car",
                newName: "Kilometros");
        }
    }
}
