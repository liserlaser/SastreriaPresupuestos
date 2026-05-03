using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SastreriaPresupuestos.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Quotes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Quotes");
        }
    }
}
