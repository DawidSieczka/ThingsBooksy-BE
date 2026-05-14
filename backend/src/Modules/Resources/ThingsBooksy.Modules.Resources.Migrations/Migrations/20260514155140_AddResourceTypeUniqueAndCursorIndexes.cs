using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThingsBooksy.Modules.Resources.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceTypeUniqueAndCursorIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_resource_types_GroupId_Name",
                schema: "resources",
                table: "resource_types",
                columns: new[] { "GroupId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resource_instances_GroupId_Id",
                schema: "resources",
                table: "resource_instances",
                columns: new[] { "GroupId", "Id" },
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_resource_types_GroupId_Name",
                schema: "resources",
                table: "resource_types");

            migrationBuilder.DropIndex(
                name: "IX_resource_instances_GroupId_Id",
                schema: "resources",
                table: "resource_instances");
        }
    }
}
