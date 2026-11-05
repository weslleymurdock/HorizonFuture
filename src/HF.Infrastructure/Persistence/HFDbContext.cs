using System;
using HF.Domain.AssetsLiabilities;
using HF.Domain.Collaborators;
using HF.Domain.Courses;
using HF.Domain.Directors;
using HF.Domain.Managers;
using HF.Domain.Students;
using HF.Domain.Teachers;
using HF.Domain.Volunteers;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HF.Infrastructure.Persistence;

public sealed class HFDbContext(DbContextOptions<HFDbContext> options) : IdentityDbContext<User, Role, string>(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Director> Directors => Set<Director>();
    public DbSet<Liability> Liabilities => Set<Liability>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(HFDbContext).Assembly);
    }
}
