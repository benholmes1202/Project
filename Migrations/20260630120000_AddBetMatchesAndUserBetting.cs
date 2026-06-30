using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Project.Services;

#nullable disable

namespace Project.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260630120000_AddBetMatchesAndUserBetting")]
    public partial class AddBetMatchesAndUserBetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BetMatches",
                columns: table => new
                {
                    BetMatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeTeam = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AwayTeam = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Sport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeOdds = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    AwayOdds = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    DrawOdds = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetMatches", x => x.BetMatchId);
                });

            migrationBuilder.AddColumn<int>(
                name: "BetMatchId",
                table: "Bets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Odds",
                table: "Bets",
                type: "decimal(16,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PotentialPayout",
                table: "Bets",
                type: "decimal(16,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Selection",
                table: "Bets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Bets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Placed");

            migrationBuilder.CreateIndex(
                name: "IX_BetMatches_IsActive",
                table: "BetMatches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BetMatches_MatchDate",
                table: "BetMatches",
                column: "MatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_BetMatches_Sport",
                table: "BetMatches",
                column: "Sport");

            migrationBuilder.CreateIndex(
                name: "IX_Bets_BetMatchId",
                table: "Bets",
                column: "BetMatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_BetMatches_BetMatchId",
                table: "Bets",
                column: "BetMatchId",
                principalTable: "BetMatches",
                principalColumn: "BetMatchId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_BetMatches_BetMatchId",
                table: "Bets");

            migrationBuilder.DropTable(
                name: "BetMatches");

            migrationBuilder.DropIndex(
                name: "IX_Bets_BetMatchId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "BetMatchId",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Odds",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "PotentialPayout",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Selection",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bets");
        }
    }
}
