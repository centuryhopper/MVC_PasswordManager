using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace passwordmanager.Migrations
{
    /// <inheritdoc />
    public partial class initialIdentityCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordTableEF_AspNetUsers_Id",
                table: "PasswordTableEF");

            migrationBuilder.DropIndex(
                name: "IX_PasswordTableEF_Id",
                table: "PasswordTableEF");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PasswordTableEF");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordTableEF_userId",
                table: "PasswordTableEF",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordTableEF_AspNetUsers_userId",
                table: "PasswordTableEF",
                column: "userId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordTableEF_AspNetUsers_userId",
                table: "PasswordTableEF");

            migrationBuilder.DropIndex(
                name: "IX_PasswordTableEF_userId",
                table: "PasswordTableEF");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "PasswordTableEF",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordTableEF_Id",
                table: "PasswordTableEF",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordTableEF_AspNetUsers_Id",
                table: "PasswordTableEF",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
