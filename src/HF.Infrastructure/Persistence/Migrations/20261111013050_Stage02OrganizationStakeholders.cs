using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HF.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Stage02OrganizationStakeholders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Volunteers",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Teachers",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Students",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Managers",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Directors",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Collaborators",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorityAssignments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    UnitId = table.Column<string>(type: "text", nullable: true),
                    Responsibility = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceBodies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceBodies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceMandates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    BodyId = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EvidenceReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceMandates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityMerges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SourceIdentityId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    TargetIdentityId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    IdentityKind = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApprovedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityMerges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstitutionIdentities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegistrationIdentifier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionIdentities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationalMemberships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    IdentityId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    IdentityKind = table.Column<int>(type: "integer", nullable: false),
                    UnitId = table.Column<string>(type: "text", nullable: true),
                    DepartmentId = table.Column<string>(type: "text", nullable: true),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationalMemberships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LegalIdentifier = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Address_Street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Address_District = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Address_State = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    Address_Complement = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    FoundedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonRelationships",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PersonId = table.Column<string>(type: "text", nullable: false),
                    RelatedPersonId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonRelationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SocialName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccountLinks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    IdentityId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    IdentityKind = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnlinkedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MembershipId = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipRoles_OrganizationalMemberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "OrganizationalMemberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationalUnits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    ParentUnitId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationalUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationalUnits_OrganizationalUnits_ParentUnitId",
                        column: x => x.ParentUnitId,
                        principalTable: "OrganizationalUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationalUnits_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<string>(type: "text", nullable: false),
                    UnitId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_OrganizationalUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "OrganizationalUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Departments_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Volunteers_PersonId",
                table: "Volunteers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_PersonId",
                table: "Teachers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_PersonId",
                table: "Students",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_PersonId",
                table: "Managers",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_PersonId",
                table: "Directors",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Collaborators_PersonId",
                table: "Collaborators",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityAssignments_OrganizationId_PersonId_Responsibility~",
                table: "AuthorityAssignments",
                columns: new[] { "OrganizationId", "PersonId", "Responsibility", "StartsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_OrganizationId_Name",
                table: "Departments",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_UnitId",
                table: "Departments",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceBodies_OrganizationId_Name",
                table: "GovernanceBodies",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceMandates_BodyId_PersonId_StartsOn",
                table: "GovernanceMandates",
                columns: new[] { "BodyId", "PersonId", "StartsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityMerges_SourceIdentityId_TargetIdentityId",
                table: "IdentityMerges",
                columns: new[] { "SourceIdentityId", "TargetIdentityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionIdentities_RegistrationIdentifier",
                table: "InstitutionIdentities",
                column: "RegistrationIdentifier",
                unique: true,
                filter: "\"RegistrationIdentifier\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRoles_MembershipId_Role_StartsOn",
                table: "MembershipRoles",
                columns: new[] { "MembershipId", "Role", "StartsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationalMemberships_OrganizationId_IdentityId_Identit~",
                table: "OrganizationalMemberships",
                columns: new[] { "OrganizationId", "IdentityId", "IdentityKind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationalUnits_OrganizationId_Name",
                table: "OrganizationalUnits",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationalUnits_ParentUnitId",
                table: "OrganizationalUnits",
                column: "ParentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_LegalIdentifier",
                table: "Organizations",
                column: "LegalIdentifier",
                unique: true,
                filter: "\"LegalIdentifier\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonRelationships_PersonId_RelatedPersonId_Type_StartsOn",
                table: "PersonRelationships",
                columns: new[] { "PersonId", "RelatedPersonId", "Type", "StartsOn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FullName_DateOfBirth",
                table: "Persons",
                columns: new[] { "FullName", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountLinks_IdentityId_IdentityKind_AccountId",
                table: "UserAccountLinks",
                columns: new[] { "IdentityId", "IdentityKind", "AccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorityAssignments");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "GovernanceBodies");

            migrationBuilder.DropTable(
                name: "GovernanceMandates");

            migrationBuilder.DropTable(
                name: "IdentityMerges");

            migrationBuilder.DropTable(
                name: "InstitutionIdentities");

            migrationBuilder.DropTable(
                name: "MembershipRoles");

            migrationBuilder.DropTable(
                name: "PersonRelationships");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "UserAccountLinks");

            migrationBuilder.DropTable(
                name: "OrganizationalUnits");

            migrationBuilder.DropTable(
                name: "OrganizationalMemberships");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Volunteers_PersonId",
                table: "Volunteers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_PersonId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_PersonId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Managers_PersonId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Directors_PersonId",
                table: "Directors");

            migrationBuilder.DropIndex(
                name: "IX_Collaborators_PersonId",
                table: "Collaborators");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Volunteers");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Directors");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Collaborators");
        }
    }
}
