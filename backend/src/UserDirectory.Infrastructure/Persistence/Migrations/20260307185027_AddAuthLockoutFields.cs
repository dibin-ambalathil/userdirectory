using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserDirectory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthLockoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "AuthUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndAt",
                table: "AuthUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "AuthUsers");

            migrationBuilder.DropColumn(
                name: "LockoutEndAt",
                table: "AuthUsers");
        }
    }
}
