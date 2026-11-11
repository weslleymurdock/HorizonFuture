using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Stage02LifecycleAndAccountConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserAccountLinks_AccountId",
                table: "UserAccountLinks",
                column: "AccountId",
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAccountLinks_AccountId",
                table: "UserAccountLinks");
        }
    }
}
