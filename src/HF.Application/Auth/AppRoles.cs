namespace HF.Application.Auth;

/// <summary>
/// Centralized catalog and hierarchy definitions for application roles.
/// </summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Director = "Director";
    public const string Coordinator = "Coordinator";
    public const string Manager = "Manager";
    public const string Coworker = "Coworker";
    public const string Volunteer = "Volunteer";
    public const string Reader = "Reader";

    public static readonly IReadOnlyList<string> All =
    [
        Admin,
        Director,
        Coordinator,
        Manager,
        Coworker,
        Volunteer,
        Reader
    ];

    /// <summary>
    /// Explicit integer hierarchy level for each role. Higher number indicates strictly greater authority.
    /// Under no circumstance should alphabetical / lexicographical sorting be used to infer authority.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int> Hierarchy = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        [Admin] = 700,
        [Director] = 600,
        [Coordinator] = 500,
        [Manager] = 400,
        [Coworker] = 300,
        [Volunteer] = 200,
        [Reader] = 100
    };

    /// <summary>
    /// Gets the integer rank of a role. Returns 0 for unknown or null roles.
    /// </summary>
    public static int GetRank(string? roleName)
    {
        if (roleName != null && Hierarchy.TryGetValue(roleName, out int rank))
        {
            return rank;
        }

        return 0;
    }

    /// <summary>
    /// Returns the maximum rank among all valid catalog roles held by a user.
    /// A user with no roles or only unknown roles receives rank 0 (no administrative authority).
    /// </summary>
    public static int GetUserMaxRank(IEnumerable<string> roles)
    {
        int maxRank = 0;
        foreach (var role in roles)
        {
            int rank = GetRank(role);
            if (rank > maxRank)
            {
                maxRank = rank;
            }
        }

        return maxRank;
    }

    /// <summary>
    /// Checks whether a given role exists in the authorized catalog.
    /// </summary>
    public static bool IsValidRole(string? roleName) =>
        roleName != null && Hierarchy.ContainsKey(roleName);
}

