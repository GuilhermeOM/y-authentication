using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y.Authentication.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserAvatarTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "UsersMetadata");

            migrationBuilder.CreateTable(
                name: "UsersAvatar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersAvatar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersAvatar_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersAvatar_UserId",
                table: "UsersAvatar",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersAvatar");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "UsersMetadata",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
