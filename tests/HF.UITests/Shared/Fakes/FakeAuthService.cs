using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;

namespace HF.UITests.Shared.Fakes;

public class FakeAuthService : IAuthService
{
    public bool ShouldSucceed { get; set; } = true;
    public string FailureMessage { get; set; } = "Operação simulada com falha";
    public int? FailureStatusCode { get; set; } = 400;
    public List<string> FailureErrors { get; set; } = new() { "Erro 1", "Erro 2" };

    public TokenResponseDto MockTokenResponse { get; set; } = new(
        "Bearer",
        "mock-access-token-xyz",
        3600,
        "mock-refresh-token-123");

    public IdentityInfoDto MockIdentityInfo { get; set; } = new(
        "user@horizon.org",
        true,
        "Usuário Teste",
        "Usuário",
        "Horizon");

    public TwoFactorResponseDto MockTwoFactorResponse { get; set; } = new(
        "MOCKSHAREDSECRETKEY2FA",
        5,
        new[] { "REC-1111", "REC-2222", "REC-3333" },
        true,
        false);

    public List<RoleCatalogItemDto> MockCatalogRoles { get; set; } = new()
    {
        new("Director", 100),
        new("Manager", 80),
        new("Teacher", 50),
        new("Collaborator", 30),
        new("Volunteer", 10)
    };

    public List<string> MockUserRoles { get; set; } = new() { "Teacher" };

    public PermissionCatalogDto MockCatalogPermissions { get; set; } = new(
        new[] { "Students.Read", "Students.Write", "Courses.Read", "Courses.Write", "Roles.Manage" },
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["AcademicStaff"] = new[] { "Students.Read", "Courses.Read" },
            ["Administrators"] = new[] { "Roles.Manage" }
        });

    public UserPermissionsDto MockUserPermissions { get; set; } = new(
        "user-test-id",
        new[] { "Students.Read", "Courses.Read" },
        new[] { "Students.Read" },
        new[] { "Students.Write" },
        new[] { "AcademicStaff" });

    public Task<AuthResult<IdentityResultDto>> RegisterAsync(RegisterDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Registrado com sucesso"));
        }
        return Task.FromResult(AuthResult<IdentityResultDto>.Fail(FailureMessage, FailureErrors, 400));
    }

    public Task<AuthResult<TokenResponseDto>> LoginAsync(LoginDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<TokenResponseDto>.Ok(MockTokenResponse, "Login bem-sucedido"));
        }
        return Task.FromResult(AuthResult<TokenResponseDto>.Fail(FailureMessage, statusCode: 401));
    }

    public Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(RefreshDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<TokenResponseDto>.Ok(MockTokenResponse, "Token renovado"));
        }
        return Task.FromResult(AuthResult<TokenResponseDto>.Fail(FailureMessage, statusCode: 401));
    }

    public Task<AuthResult> RevokeTokenAsync(CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult.Ok("Token revogado"));
        }
        return Task.FromResult(AuthResult.Fail(FailureMessage, statusCode: 401));
    }

    public Task<AuthResult<bool>> ConfirmEmailAsync(string userId, string code, string? changedEmail = null, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<bool>.Ok(true, "Email confirmado"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<IdentityResultDto>> ResendConfirmationEmailAsync(EmailDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Email reenviado"));
        }
        return Task.FromResult(AuthResult<IdentityResultDto>.Fail(FailureMessage, FailureErrors, 400));
    }

    public Task<AuthResult<IdentityResultDto>> ForgotPasswordAsync(EmailDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Recuperação enviada"));
        }
        return Task.FromResult(AuthResult<IdentityResultDto>.Fail(FailureMessage, FailureErrors, 400));
    }

    public Task<AuthResult<IdentityResultDto>> ResetPasswordAsync(ResetPasswordDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Senha redefinida"));
        }
        return Task.FromResult(AuthResult<IdentityResultDto>.Fail(FailureMessage, FailureErrors, 400));
    }

    public Task<AuthResult<IdentityInfoDto>> GetIdentityInfoAsync(CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IdentityInfoDto>.Ok(MockIdentityInfo));
        }
        return Task.FromResult(AuthResult<IdentityInfoDto>.Fail(FailureMessage, statusCode: 404));
    }

    public Task<AuthResult<IdentityResultDto>> UpdateIdentityInfoAsync(InfoDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IdentityResultDto>.Ok(new IdentityResultDto(true, Array.Empty<string>()), "Informações atualizadas"));
        }
        return Task.FromResult(AuthResult<IdentityResultDto>.Fail(FailureMessage, FailureErrors, 400));
    }

    public Task<AuthResult<TwoFactorResponseDto>> ConfigureTwoFactorAsync(TwoFactorDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<TwoFactorResponseDto>.Ok(MockTwoFactorResponse));
        }
        return Task.FromResult(AuthResult<TwoFactorResponseDto>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<IReadOnlyList<RoleCatalogItemDto>>> ListCatalogRolesAsync(CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Ok(MockCatalogRoles));
        }
        return Task.FromResult(AuthResult<IReadOnlyList<RoleCatalogItemDto>>.Fail(FailureMessage, statusCode: 403));
    }

    public Task<AuthResult<UserRolesDto>> GetUserRolesAsync(string userId, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<UserRolesDto>.Ok(new UserRolesDto(userId, MockUserRoles)));
        }
        return Task.FromResult(AuthResult<UserRolesDto>.Fail(FailureMessage, statusCode: 404));
    }

    public Task<AuthResult<bool>> AssignUserRoleAsync(string userId, AssignRoleDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            if (!MockUserRoles.Contains(request.RoleName))
            {
                MockUserRoles.Add(request.RoleName);
            }
            return Task.FromResult(AuthResult<bool>.Ok(true, "Role atribuída"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<bool>> RemoveUserRoleAsync(string userId, string roleName, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            MockUserRoles.Remove(roleName);
            return Task.FromResult(AuthResult<bool>.Ok(true, "Role removida"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<PermissionCatalogDto>> ListCatalogPermissionsAsync(CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<PermissionCatalogDto>.Ok(MockCatalogPermissions));
        }
        return Task.FromResult(AuthResult<PermissionCatalogDto>.Fail(FailureMessage, statusCode: 403));
    }

    public Task<AuthResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<UserPermissionsDto>.Ok(MockUserPermissions));
        }
        return Task.FromResult(AuthResult<UserPermissionsDto>.Fail(FailureMessage, statusCode: 404));
    }

    public Task<AuthResult<bool>> GrantUserPermissionAsync(string userId, GrantPermissionDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<bool>.Ok(true, "Permissão concedida"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<bool>> RevokeUserPermissionAsync(string userId, RevokePermissionDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<bool>.Ok(true, "Permissão revogada"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<bool>> ClearUserPermissionAsync(string userId, string permissionName, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<bool>.Ok(true, "Override de permissão limpo"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<bool>> AssignUserPermissionGroupAsync(string userId, AssignPermissionGroupDto request, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<bool>.Ok(true, "Grupo atribuído"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public Task<AuthResult<bool>> RemoveUserPermissionGroupAsync(string userId, string groupName, CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<bool>.Ok(true, "Grupo removido"));
        }
        return Task.FromResult(AuthResult<bool>.Fail(FailureMessage, statusCode: 400));
    }

    public List<string> MockUserPolicies { get; set; } = new() { "Courses.Read", "Subjects.Read", "Students.Read", "Enrollments.Read" };

    public Task<AuthResult<UserAccessDto>> GetCurrentUserAccessAsync(CancellationToken ct = default)
    {
        if (ShouldSucceed)
        {
            return Task.FromResult(AuthResult<UserAccessDto>.Ok(
                new UserAccessDto(MockUserRoles, MockUserPolicies),
                "Acesso obtido com sucesso"));
        }
        return Task.FromResult(AuthResult<UserAccessDto>.Fail(FailureMessage, statusCode: FailureStatusCode ?? 400));
    }
}


