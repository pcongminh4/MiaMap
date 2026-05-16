using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIdsToNodesAndRoads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "roads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "roads",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at_utc",
                table: "roads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "nodes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "nodes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at_utc",
                table: "nodes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_roads_source_external_id",
                table: "roads",
                columns: new[] { "source", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nodes_source_external_id",
                table: "nodes",
                columns: new[] { "source", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roads_source_external_id",
                table: "roads");

            migrationBuilder.DropIndex(
                name: "IX_nodes_source_external_id",
                table: "nodes");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "roads");

            migrationBuilder.DropColumn(
                name: "source",
                table: "roads");

            migrationBuilder.DropColumn(
                name: "updated_at_utc",
                table: "roads");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "nodes");

            migrationBuilder.DropColumn(
                name: "source",
                table: "nodes");

            migrationBuilder.DropColumn(
                name: "updated_at_utc",
                table: "nodes");
        }
    }
}
