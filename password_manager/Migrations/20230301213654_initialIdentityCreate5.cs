using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace passwordmanager.Migrations
{
    /// <inheritdoc />
    public partial class initialIdentityCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8afacf2e-fa7a-4cdc-83cd-eeb13346d589", null, "Regular_User", "REGULAR_USER" },
                    { "8c768236-5103-485b-86d5-7e5913fa548e", null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "6fdd839a-d282-4bdd-a2a6-60a300770134", 0, "3cda5829-ed85-4a21-a3bd-42f605d9a795", "dummyreceiver66@gmail.com", true, "regular_user_first_name", "regular_user_last_name", false, null, "DUMMYRECEIVER66@GMAIL.COM", "REGULAR_USER", "AQAAAAIAAYagAAAAEA0yRmLmkVeQ0cwFMM02sChFrxNAP3WTCM+U0IK9hfjv+rKxGV6rShNSvxt9voO6Zg==", null, false, "bf9adec4-7f33-489f-917c-49c41039aafa", false, "Regular_User" },
                    { "bbae7069-d1c8-40af-89e9-c881d821394c", 0, "594d4b80-a3e2-4e4b-85b1-c7543c6657f4", "boviner1990@gmail.com", true, "admin_first_name", "admin_last_name", false, null, "BOVINER1990@GMAIL.COM", "ADMIN", "AQAAAAIAAYagAAAAEBu8I/wTGa8DF3v+kH4cqDWEyNv+180aW2KUl30wJXVfJtdb2wNn6mg3tJR0QNgOgg==", null, false, "d9b0253d-1155-41a7-9d76-b793c1fac35b", false, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "8afacf2e-fa7a-4cdc-83cd-eeb13346d589", "6fdd839a-d282-4bdd-a2a6-60a300770134" },
                    { "8c768236-5103-485b-86d5-7e5913fa548e", "bbae7069-d1c8-40af-89e9-c881d821394c" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8afacf2e-fa7a-4cdc-83cd-eeb13346d589", "6fdd839a-d282-4bdd-a2a6-60a300770134" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8c768236-5103-485b-86d5-7e5913fa548e", "bbae7069-d1c8-40af-89e9-c881d821394c" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8afacf2e-fa7a-4cdc-83cd-eeb13346d589");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8c768236-5103-485b-86d5-7e5913fa548e");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "6fdd839a-d282-4bdd-a2a6-60a300770134");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "bbae7069-d1c8-40af-89e9-c881d821394c");
        }
    }
}
