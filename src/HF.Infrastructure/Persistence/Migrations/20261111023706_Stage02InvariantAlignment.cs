using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Stage02InvariantAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Departments_OrganizationId_Name",
                table: "Departments");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId_UnitId_Name",
                table: "Departments",
                columns: new[] { "OrganizationId", "UnitId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Departments_OrganizationId_UnitId_Name",
                table: "Departments");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId_Name",
                table: "Departments",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);
        }
    }
}
