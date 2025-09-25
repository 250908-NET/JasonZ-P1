using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardDeck.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Target = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false,
                        defaultValue: 21.0m
                    ),
                    DrawLimit = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Money = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false,
                        defaultValue: 1000.0m
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false,
                        defaultValueSql: "SYSDATETIMEOFFSET()"
                    ),
                    Round = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Bet = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false,
                        defaultValue: 0.0m
                    ),
                    Status = table.Column<byte>(
                        type: "tinyint",
                        nullable: false,
                        defaultValue: (byte)0
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Suits",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(
                        type: "nvarchar(15)",
                        maxLength: 15,
                        nullable: false
                    ),
                    Symbol = table.Column<string>(type: "nchar(1)", nullable: false),
                    ColorRGB = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suits", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rank = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    SuitId = table.Column<int>(type: "int", nullable: false),
                    Effects = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false,
                        defaultValueSql: "SYSDATETIMEOFFSET()"
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Suits_SuitId",
                        column: x => x.SuitId,
                        principalTable: "Suits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "AvailableCards",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false,
                        defaultValueSql: "SYSDATETIMEOFFSET()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailableCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvailableCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "GameCards",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CardId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerType = table.Column<byte>(
                        type: "tinyint",
                        nullable: false,
                        defaultValue: (byte)0
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_GameCards_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AvailableCards_CardId",
                table: "AvailableCards",
                column: "CardId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Rank_SuitId",
                table: "Cards",
                columns: new[] { "Rank", "SuitId" },
                unique: true
            );

            migrationBuilder.CreateIndex(name: "IX_Cards_SuitId", table: "Cards", column: "SuitId");

            migrationBuilder.CreateIndex(
                name: "IX_GameCards_CardId",
                table: "GameCards",
                column: "CardId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_GameCards_GameId",
                table: "GameCards",
                column: "GameId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Suits_Name",
                table: "Suits",
                column: "Name",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AvailableCards");

            migrationBuilder.DropTable(name: "GameCards");

            migrationBuilder.DropTable(name: "Cards");

            migrationBuilder.DropTable(name: "Games");

            migrationBuilder.DropTable(name: "Suits");
        }
    }
}
