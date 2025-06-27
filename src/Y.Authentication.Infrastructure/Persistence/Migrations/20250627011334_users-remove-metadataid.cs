using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y.Authentication.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class usersremovemetadataid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserMetadataId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserMetadataId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
