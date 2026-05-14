using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThingsBooksy.Modules.Users.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenRevocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "token_revocations",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Jti = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token_revocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_token_revocations_Jti",
                schema: "users",
                table: "token_revocations",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_token_revocations_UserId_RevokedAt",
                schema: "users",
                table: "token_revocations",
                columns: new[] { "UserId", "RevokedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "token_revocations",
                schema: "users");
        }
    }
}
