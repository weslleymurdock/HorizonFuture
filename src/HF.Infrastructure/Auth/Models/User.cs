using HF.Domain.Common.Entities;
using Microsoft.AspNetCore.Identity;

namespace HF.Infrastructure.Auth.Models;

public class User : IdentityUser<string>, IEntityBase
{
    public User() : base()
    {
        Id = Guid.NewGuid().ToString();
    }

    public User(string userName) : base(userName)
    {
        Id = Guid.NewGuid().ToString();
    }

    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? SurName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; } = string.Empty;
}
