using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThingsBooksy.Modules.ManagementGroups.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupOwnerNameUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagementGroups_Name",
                schema: "management_groups",
                table: "ManagementGroups");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementGroups_OwnerId_Name",
                schema: "management_groups",
                table: "ManagementGroups",
                columns: new[] { "OwnerId", "Name" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagementGroups_OwnerId_Name",
                schema: "management_groups",
                table: "ManagementGroups");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementGroups_Name",
                schema: "management_groups",
                table: "ManagementGroups",
                column: "Name",
                unique: true);
        }
    }
}
