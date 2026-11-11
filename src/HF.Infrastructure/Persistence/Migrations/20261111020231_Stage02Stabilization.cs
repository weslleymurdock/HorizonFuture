using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Stage02Stabilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "IdentityMerges",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "IdentityMerges",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAtUtc",
                table: "IdentityMerges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedBy",
                table: "IdentityMerges",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "IdentityMerges");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "IdentityMerges");

            migrationBuilder.DropColumn(
                name: "RejectedAtUtc",
                table: "IdentityMerges");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "IdentityMerges");
        }
    }
}
