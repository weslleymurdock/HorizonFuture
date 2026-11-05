using HF.Shared.Auth.Models;

namespace HF.Shared.Auth;

/// <summary>
/// Contrato do serviço cliente para todos os endpoints do controller Auth da API.
/// </summary>
public interface IAuthService
{
    // 1. POST api/v1/auth/register
    Task<AuthResult<IdentityResultDto>> RegisterAsync(RegisterDto request, CancellationToken ct = default);

    // 2. POST api/v1/auth/login
    Task<AuthResult<TokenResponseDto>> LoginAsync(LoginDto request, CancellationToken ct = default);

    // 3. POST api/v1/auth/refresh
    Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(RefreshDto request, CancellationToken ct = default);

    // 4. POST api/v1/auth/revoke
    Task<AuthResult> RevokeTokenAsync(CancellationToken ct = default);

    // 5. GET api/v1/auth/confirmEmail
    Task<AuthResult<bool>> ConfirmEmailAsync(string userId, string code, string? changedEmail = null, CancellationToken ct = default);

    // 6. POST api/v1/auth/resendConfirmationEmail
    Task<AuthResult<IdentityResultDto>> ResendConfirmationEmailAsync(EmailDto request, CancellationToken ct = default);

    // 7. POST api/v1/auth/forgotPassword
    Task<AuthResult<IdentityResultDto>> ForgotPasswordAsync(EmailDto request, CancellationToken ct = default);

    // 8. POST api/v1/auth/resetPassword
    Task<AuthResult<IdentityResultDto>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken ct = default);

    // 9. GET api/v1/auth/manage/info
    Task<AuthResult<IdentityInfoDto>> GetIdentityInfoAsync(CancellationToken ct = default);

    // 10. POST api/v1/auth/manage/info
    Task<AuthResult<IdentityResultDto>> UpdateIdentityInfoAsync(InfoDto request, CancellationToken ct = default);

    // 11. POST api/v1/auth/manage/2fa
    Task<AuthResult<TwoFactorResponseDto>> ConfigureTwoFactorAsync(TwoFactorDto request, CancellationToken ct = default);

    // 12. GET api/v1/auth/roles
    Task<AuthResult<IReadOnlyList<RoleCatalogItemDto>>> ListCatalogRolesAsync(CancellationToken ct = default);

    // 13. GET api/v1/auth/users/{userId}/roles
    Task<AuthResult<UserRolesDto>> GetUserRolesAsync(string userId, CancellationToken ct = default);

    // 14. POST api/v1/auth/users/{userId}/roles
    Task<AuthResult<bool>> AssignUserRoleAsync(string userId, AssignRoleDto request, CancellationToken ct = default);

    // 15. DELETE api/v1/auth/users/{userId}/roles/{roleName}
    Task<AuthResult<bool>> RemoveUserRoleAsync(string userId, string roleName, CancellationToken ct = default);

    // 16. GET api/v1/auth/permissions
    Task<AuthResult<PermissionCatalogDto>> ListCatalogPermissionsAsync(CancellationToken ct = default);

    // 17. GET api/v1/auth/users/{userId}/permissions
    Task<AuthResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

    // 18. POST api/v1/auth/users/{userId}/permissions/grant
    Task<AuthResult<bool>> GrantUserPermissionAsync(string userId, GrantPermissionDto request, CancellationToken ct = default);

    // 19. POST api/v1/auth/users/{userId}/permissions/revoke
    Task<AuthResult<bool>> RevokeUserPermissionAsync(string userId, RevokePermissionDto request, CancellationToken ct = default);

    // 20. DELETE api/v1/auth/users/{userId}/permissions/{permissionName}
    Task<AuthResult<bool>> ClearUserPermissionAsync(string userId, string permissionName, CancellationToken ct = default);

    // 21. POST api/v1/auth/users/{userId}/groups
    Task<AuthResult<bool>> AssignUserPermissionGroupAsync(string userId, AssignPermissionGroupDto request, CancellationToken ct = default);

    // 22. DELETE api/v1/auth/users/{userId}/groups/{groupName}
    Task<AuthResult<bool>> RemoveUserPermissionGroupAsync(string userId, string groupName, CancellationToken ct = default);
}

