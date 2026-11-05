namespace HF.Application.Auth.Abstract;

/// <summary>
/// Service abstraction to evaluate user permissions dynamically per request.
/// </summary>
public interface IUserPermissionEvaluator
{
    /// <summary>
    /// Evaluates whether a user has the specified permission, checking database-backed claims and roles.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="permission">The permission name to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is authorized; otherwise false.</returns>
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
}

