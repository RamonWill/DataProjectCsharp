using Microsoft.EntityFrameworkCore.Migrations;

namespace DataProjectCsharp.Migrations
{
    public partial class InsertVisitorRoletoDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f66aed88-234f-4d95-9dba-436fbf324f95");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "fc661ac3-5a00-4a93-a595-fa09169dc362");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "11044e7e-2f5c-4165-adaf-6ba815502961", "f2c5bb60-ae94-45cf-9432-0f1bcc3ec831", "Visitor", "Visitor" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "476e0cd4-4610-4d23-a60c-840240c28db6", "ad30645f-fc4c-4ffc-b871-69f194ca720b", "Administrator", "ADMIN" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11044e7e-2f5c-4165-adaf-6ba815502961");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "476e0cd4-4610-4d23-a60c-840240c28db6");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "f66aed88-234f-4d95-9dba-436fbf324f95", "b38e9aa1-c995-442e-8192-61c45416efc7", "DummyUser", "DUMMYUSER" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "fc661ac3-5a00-4a93-a595-fa09169dc362", "f137c88a-b689-41ba-90f8-1fc76196b731", "Administrator", "ADMIN" });
        }
    }
}
