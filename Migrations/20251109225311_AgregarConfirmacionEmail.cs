using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiFarmacia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfirmacionEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmacionToken",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailConfirmacionTokenExpiracion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiracion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmailConfirmacionToken",
                table: "Usuarios",
                column: "EmailConfirmacionToken");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PasswordResetToken",
                table: "Usuarios",
                column: "PasswordResetToken");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RefreshToken",
                table: "Usuarios",
                column: "RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmailConfirmacionToken",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PasswordResetToken",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_RefreshToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailConfirmacionToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EmailConfirmacionTokenExpiracion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiracion",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
