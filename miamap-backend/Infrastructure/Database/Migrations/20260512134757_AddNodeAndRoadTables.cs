using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeAndRoadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nodes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    location = table.Column<Point>(type: "geometry(point, 4326)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nodes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roads",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    geometry = table.Column<LineString>(type: "geometry(linestring, 4326)", nullable: false),
                    length_meters = table.Column<double>(type: "double precision", nullable: false),
                    start_node_id = table.Column<int>(type: "integer", nullable: false),
                    end_node_id = table.Column<int>(type: "integer", nullable: false),
                    is_one_way = table.Column<bool>(type: "boolean", nullable: false),
                    road_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    road_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    max_speed_kmh = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<double>(type: "double precision", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roads", x => x.id);
                    table.ForeignKey(
                        name: "FK_roads_nodes_end_node_id",
                        column: x => x.end_node_id,
                        principalTable: "nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roads_nodes_start_node_id",
                        column: x => x.start_node_id,
                        principalTable: "nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roads_end_node_id",
                table: "roads",
                column: "end_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_roads_start_node_id",
                table: "roads",
                column: "start_node_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roads");

            migrationBuilder.DropTable(
                name: "nodes");
        }
    }
}
