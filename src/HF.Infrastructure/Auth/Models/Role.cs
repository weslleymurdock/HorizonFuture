using Microsoft.AspNetCore.Identity;

namespace HF.Infrastructure.Auth.Models;

public class Role(string name = "") : IdentityRole<string>(name)
{
}
