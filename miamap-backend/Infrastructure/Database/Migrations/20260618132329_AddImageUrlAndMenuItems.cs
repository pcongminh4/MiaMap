using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlAndMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportId1",
                table: "report_votes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "places",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    place_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_menu_items_places_place_id",
                        column: x => x.place_id,
                        principalTable: "places",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_votes_ReportId1",
                table: "report_votes",
                column: "ReportId1");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_name",
                table: "menu_items",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_menu_items_place_id",
                table: "menu_items",
                column: "place_id");

            migrationBuilder.AddForeignKey(
                name: "FK_report_votes_reports_ReportId1",
                table: "report_votes",
                column: "ReportId1",
                principalTable: "reports",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_report_votes_reports_ReportId1",
                table: "report_votes");

            migrationBuilder.DropTable(
                name: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_report_votes_ReportId1",
                table: "report_votes");

            migrationBuilder.DropColumn(
                name: "ReportId1",
                table: "report_votes");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "places");
        }
    }
}
