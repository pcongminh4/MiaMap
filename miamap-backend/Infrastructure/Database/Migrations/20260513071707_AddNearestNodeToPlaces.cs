using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNearestNodeToPlaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "nearest_node_id",
                table: "places",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_places_nearest_node_id",
                table: "places",
                column: "nearest_node_id");

            migrationBuilder.AddForeignKey(
                name: "FK_places_nodes_nearest_node_id",
                table: "places",
                column: "nearest_node_id",
                principalTable: "nodes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_places_nodes_nearest_node_id",
                table: "places");

            migrationBuilder.DropIndex(
                name: "IX_places_nearest_node_id",
                table: "places");

            migrationBuilder.DropColumn(
                name: "nearest_node_id",
                table: "places");
        }
    }
}
