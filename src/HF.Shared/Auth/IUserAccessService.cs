using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HF.Shared.Auth;

/// <summary>
/// Service managing the active user's roles and effective authorization policies in memory.
/// Synchronizes capabilities after login, refresh, session recovery, and administrative changes.
/// </summary>
public interface IUserAccessService
{
    IReadOnlyList<string> CurrentRoles { get; }
    IReadOnlyList<string> CurrentPolicies { get; }
    bool IsLoaded { get; }

    bool HasRole(string role);
    bool HasPolicy(string policy);

    Task<bool> LoadCurrentAccessAsync(CancellationToken ct = default);
    void SetAccess(IEnumerable<string> roles, IEnumerable<string> policies);
    void ClearAccess();

    event EventHandler? AccessChanged;
}

