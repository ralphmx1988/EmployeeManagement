using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "CreatedAt", "Department", "Email", "FirstName", "HireDate", "IsActive", "LastName", "PhoneNumber", "Position", "Salary", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 24, 22, 35, 10, 347, DateTimeKind.Utc).AddTicks(9194), "Navigation", "captain.smith@cruiseline.com", "Captain", new DateTime(2020, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Smith", "+1-555-0101", "Ship Captain", 120000m, null },
                    { 2, new DateTime(2025, 7, 24, 22, 35, 10, 347, DateTimeKind.Utc).AddTicks(9277), "Engineering", "sarah.johnson@cruiseline.com", "Sarah", new DateTime(2019, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Johnson", "+1-555-0102", "Chief Engineer", 95000m, null },
                    { 3, new DateTime(2025, 7, 24, 22, 35, 10, 347, DateTimeKind.Utc).AddTicks(9279), "Security", "mike.davis@cruiseline.com", "Mike", new DateTime(2021, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Davis", "+1-555-0103", "Security Chief", 75000m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Department",
                table: "Employees",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsActive",
                table: "Employees",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Position",
                table: "Employees",
                column: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
