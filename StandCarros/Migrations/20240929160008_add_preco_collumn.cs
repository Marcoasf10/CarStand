using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StandCarros.Migrations
{
    /// <inheritdoc />
    public partial class add_preco_collumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Preco",
                table: "Car",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preco",
                table: "Car");
        }
    }
}
