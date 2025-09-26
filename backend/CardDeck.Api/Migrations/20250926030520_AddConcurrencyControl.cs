using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Suits",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Games",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Cards",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AvailableCards",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Suits");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AvailableCards");
        }
    }
}
