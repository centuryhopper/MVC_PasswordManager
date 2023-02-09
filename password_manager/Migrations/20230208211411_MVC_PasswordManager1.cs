using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace passwordmanager.Migrations
{
    /// <inheritdoc />
    public partial class MVCPasswordManager1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTableEF",
                columns: table => new
                {
                    userId = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    aesKey = table.Column<string>(type: "text", nullable: true),
                    aesIV = table.Column<string>(type: "text", nullable: true),
                    currentJwtToken = table.Column<string>(type: "text", nullable: false),
                    tokenCreated = table.Column<string>(type: "text", nullable: true),
                    tokenExpires = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTableEF", x => x.userId);
                });

            migrationBuilder.CreateTable(
                name: "PasswordTableEF",
                columns: table => new
                {
                    accountId = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    password = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    aesKey = table.Column<string>(type: "text", nullable: true),
                    aesIV = table.Column<string>(type: "text", nullable: true),
                    insertedDateTime = table.Column<string>(type: "text", nullable: true),
                    lastModifiedDateTime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordTableEF", x => x.accountId);
                    table.ForeignKey(
                        name: "FK_PasswordTableEF_UserTableEF_userId",
                        column: x => x.userId,
                        principalTable: "UserTableEF",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordTableEF_userId",
                table: "PasswordTableEF",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordTableEF");

            migrationBuilder.DropTable(
                name: "UserTableEF");
        }
    }
}
