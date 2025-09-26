using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class IForgor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DrawLimit",
                table: "Games",
                newName: "OverdrawLimit");

            migrationBuilder.AddColumn<int>(
                name: "OverdrawsRemaining",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "GameId1",
                table: "GameCards",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameCards_GameId1",
                table: "GameCards",
                column: "GameId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GameCards_Games_GameId1",
                table: "GameCards",
                column: "GameId1",
                principalTable: "Games",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameCards_Games_GameId1",
                table: "GameCards");

            migrationBuilder.DropIndex(
                name: "IX_GameCards_GameId1",
                table: "GameCards");

            migrationBuilder.DropColumn(
                name: "OverdrawsRemaining",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameId1",
                table: "GameCards");

            migrationBuilder.RenameColumn(
                name: "OverdrawLimit",
                table: "Games",
                newName: "DrawLimit");
        }
    }
}
