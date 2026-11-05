using Microsoft.AspNetCore.Identity;

namespace HF.Infrastructure.Auth.Models;

public class Role : IdentityRole<string>
{
    public Role() : base()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Role(string name) : base(name)
    {
        Id = Guid.NewGuid().ToString();
    }
}
