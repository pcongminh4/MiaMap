using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRawOsmStagingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "raw_osm_nodes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    location = table.Column<Point>(type: "geometry(point, 4326)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    tags = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_osm_nodes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "raw_osm_places",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    location = table.Column<Point>(type: "geometry(point, 4326)", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tags = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_osm_places", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "raw_osm_ways",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    node_ids = table.Column<long[]>(type: "bigint[]", nullable: false),
                    tags = table.Column<string>(type: "jsonb", nullable: true),
                    highway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    oneway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    maxspeed = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raw_osm_ways", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_raw_osm_nodes_location",
                table: "raw_osm_nodes",
                column: "location");

            migrationBuilder.CreateIndex(
                name: "ix_raw_osm_places_location",
                table: "raw_osm_places",
                column: "location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "raw_osm_nodes");

            migrationBuilder.DropTable(
                name: "raw_osm_places");

            migrationBuilder.DropTable(
                name: "raw_osm_ways");
        }
    }
}
