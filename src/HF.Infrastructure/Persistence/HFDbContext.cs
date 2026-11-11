using System;
using HF.Domain.AssetsLiabilities;
using HF.Domain.Attendances;
using HF.Domain.ClassSessions;
using HF.Domain.Collaborators;
using HF.Domain.Courses;
using HF.Domain.Directors;
using HF.Domain.Enrollments;
using HF.Domain.Managers;
using HF.Domain.Governance;
using HF.Domain.Organization;
using HF.Domain.Students;
using HF.Domain.Stakeholders;
using HF.Domain.TeacherAssignments;
using HF.Domain.Teachers;
using HF.Domain.Volunteers;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HF.Infrastructure.Persistence;

public sealed class HFDbContext(DbContextOptions<HFDbContext> options) : IdentityDbContext<User, Role, string>(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Director> Directors => Set<Director>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Liability> Liabilities => Set<Liability>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationalUnit> OrganizationalUnits => Set<OrganizationalUnit>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<InstitutionIdentity> InstitutionIdentities => Set<InstitutionIdentity>();
    public DbSet<OrganizationalMembership> OrganizationalMemberships => Set<OrganizationalMembership>();
    public DbSet<MembershipRole> MembershipRoles => Set<MembershipRole>();
    public DbSet<UserAccountLink> UserAccountLinks => Set<UserAccountLink>();
    public DbSet<PersonRelationship> PersonRelationships => Set<PersonRelationship>();
    public DbSet<GovernanceBody> GovernanceBodies => Set<GovernanceBody>();
    public DbSet<GovernanceMandate> GovernanceMandates => Set<GovernanceMandate>();
    public DbSet<AuthorityAssignment> AuthorityAssignments => Set<AuthorityAssignment>();
    public DbSet<IdentityMerge> IdentityMerges => Set<IdentityMerge>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherAssignment> TeacherAssignments => Set<TeacherAssignment>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(HFDbContext).Assembly);
    }
}
