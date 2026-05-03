using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SastreriaPresupuestos.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Deposit",
                table: "Quotes",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deposit",
                table: "Quotes");
        }
    }
}
