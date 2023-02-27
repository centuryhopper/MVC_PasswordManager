using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace passwordmanager.Migrations
{
    /// <inheritdoc />
    public partial class initialIdentityCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordTableEF_AspNetUsers_userId",
                table: "PasswordTableEF");

            migrationBuilder.DropIndex(
                name: "IX_PasswordTableEF_userId",
                table: "PasswordTableEF");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "PasswordTableEF",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "userId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

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
    }
}
