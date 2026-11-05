using System;
using HF.Infrastructure.Auth.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HF.Infrastructure.Persistence;

public sealed class HFDbContext(DbContextOptions<HFDbContext> options) : IdentityDbContext<User, Role, string>(options)
{

}
