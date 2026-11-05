namespace HF.Shared.Auth.Models;

/// <summary>DTO para registro de novo usuário.</summary>
public sealed record RegisterDto(string Email, string Password);

/// <summary>DTO para autenticação de usuário.</summary>
public sealed record LoginDto(
    string Email,
    string Password,
    string? TwoFactorCode = null,
    string? TwoFactorRecoveryCode = null);

/// <summary>DTO para renovação de token.</summary>
public sealed record RefreshDto(string RefreshToken);

/// <summary>DTO para operações baseadas apenas em email.</summary>
public sealed record EmailDto(string Email);

/// <summary>DTO para redefinição de senha com código.</summary>
public sealed record ResetPasswordDto(string Email, string ResetCode, string NewPassword);

/// <summary>DTO para atualização de informações cadastrais e senha.</summary>
public sealed record InfoDto(string? NewEmail, string? NewPassword, string OldPassword);

/// <summary>DTO para configuração de autenticação em dois fatores.</summary>
public sealed record TwoFactorDto(
    bool? Enable = null,
    string? TwoFactorCode = null,
    bool ResetRecoveryCodes = false,
    bool ResetSharedKey = false,
    bool ForgetMachine = false);

/// <summary>DTO para atribuição de role a um usuário.</summary>
public sealed record AssignRoleDto(string RoleName);

/// <summary>DTO para concessão direta de permissão a um usuário.</summary>
public sealed record GrantPermissionDto(string PermissionName);

/// <summary>DTO para revogação explícita de permissão de um usuário.</summary>
public sealed record RevokePermissionDto(string PermissionName);

/// <summary>DTO para associação de usuário a um grupo de permissões.</summary>
public sealed record AssignPermissionGroupDto(string GroupName);

/// <summary>Resposta de tokens de autenticação.</summary>
public sealed record TokenResponseDto(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

/// <summary>Resultado de operações de identidade.</summary>
public sealed record IdentityResultDto(
    bool Succeeded,
    IReadOnlyCollection<string> Errors);

/// <summary>Informações do perfil da identidade autenticada.</summary>
public sealed record IdentityInfoDto(
    string Email,
    bool IsEmailConfirmed,
    string? DisplayName = null,
    string? FirstName = null,
    string? SurName = null);

/// <summary>Configuração e status de autenticação em dois fatores.</summary>
public sealed record TwoFactorResponseDto(
    string? SharedKey,
    int RecoveryCodesLeft,
    IReadOnlyCollection<string>? RecoveryCodes,
    bool IsTwoFactorEnabled,
    bool IsMachineRemembered);

/// <summary>Item do catálogo de roles com hierarquia.</summary>
public sealed record RoleCatalogItemDto(string RoleName, int Rank);

/// <summary>Roles atribuídas a um usuário.</summary>
public sealed record UserRolesDto(
    string UserId,
    IReadOnlyList<string> Roles);

/// <summary>Permissões e grupos do catálogo.</summary>
public sealed record PermissionCatalogDto(
    IReadOnlyList<string> Permissions,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Groups);

/// <summary>Permissões efetivas, concessões, revogações e grupos de um usuário.</summary>
public sealed record UserPermissionsDto(
    string UserId,
    IReadOnlyList<string> EffectivePermissions,
    IReadOnlyList<string> DirectGrants,
    IReadOnlyList<string> ExplicitRevocations,
    IReadOnlyList<string> AssignedGroups);

/// <summary>Roles e policies efetivas do usuário autenticado atual.</summary>
public sealed record UserAccessDto(
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Policies);

/// <summary>Envelope genérico de resposta da API.</summary>
public sealed record ApiResponseEnvelope<T>(
    T? Value,
    string Message,
    bool Success);

