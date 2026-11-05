using Mediator;
using HF.Application.Auth;
using HF.Application.Auth.Dtos;
using HF.Application.Auth.Requests;
using HF.Application.Auth.Responses;
using HF.Application.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HF.API.Controllers.v1;

/// <summary>
/// Provides endpoints for authentication, external identity federation, and identity management.
/// </summary>
/// <param name="mediator">Mediator used to dispatch authentication and identity requests.</param>
[Route("api/v1/auth")]
[ApiController]
[Tags("Authentication")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>Registers a new user.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType<IdentityResultResponse>(400)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        IdentityResultResponse result = await mediator.Send(
            new RegisterRequest(
                dto.Email,
                dto.Password),
            cancellationToken);
        return result.Succeeded
            ? Ok()
            : BadRequest(result);
    }

    /// <summary>Authenticates a user.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<TokenResponse>(200)]
    [ProducesResponseType<ResponseBase<TokenResponse>>(401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        ResponseBase<TokenResponse> result =
            await mediator.Send(
                new LoginRequest(
                    dto.Email,
                    dto.Password,
                    dto.TwoFactorCode,
                    dto.TwoFactorRecoveryCode),
                cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : Unauthorized(result);
    }

 

    /// <summary>Refreshes authentication tokens.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<TokenResponse>(200)]
    [ProducesResponseType<ResponseBase<TokenResponse>>(401)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshDto dto,
        CancellationToken cancellationToken)
    {
        ResponseBase<TokenResponse> result = await mediator.Send(
            new RefreshTokenRequest(dto.RefreshToken),
            cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : Unauthorized(result);
    }

    /// <summary>Revokes the current access token.</summary>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType<ResponseBase<bool>>(401)]
    public async Task<IActionResult> Revoke(
        CancellationToken cancellationToken)
    {
        string token = Request.Headers.Authorization
            .ToString()
            .Replace(
                "Bearer ",
                string.Empty,
                StringComparison.OrdinalIgnoreCase)
            .Trim();

        ResponseBase<bool> result = await mediator.Send(
            new RevokeTokenRequest(token),
            cancellationToken);

        return result.Value == true
            ? Ok()
            : Unauthorized(result);
    }

    /// <summary>Confirms a user's email.</summary>
    [HttpGet("confirmEmail")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType<ResponseBase<bool>>(400)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string code,
        [FromQuery] string? changedEmail,
        CancellationToken cancellationToken)
    {
        ResponseBase<bool> result = await mediator.Send(
            new ConfirmEmailRequest(
                userId,
                code,
                changedEmail),
            cancellationToken);
        return result.Value == true
            ? Ok()
            : BadRequest(result);
    }

    /// <summary>Resends the confirmation email.</summary>
    [HttpPost("resendConfirmationEmail")]
    [AllowAnonymous]
    [ProducesResponseType<IdentityResultResponse>(200)]
    [ProducesResponseType<IdentityResultResponse>(400)]
    public async Task<IActionResult> ResendConfirmationEmail(
        [FromBody] EmailDto dto,
        CancellationToken cancellationToken)
    {
        IdentityResultResponse result = await mediator.Send(
            new ResendConfirmationEmailRequest(
                dto.Email),
            cancellationToken);
        return result.Succeeded
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Starts password recovery.</summary>
    [HttpPost("forgotPassword")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType<IdentityResultResponse>(400)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] EmailDto dto,
        CancellationToken cancellationToken)
    {
        IdentityResultResponse result = await mediator.Send(
            new ForgotPasswordRequest(dto.Email),
            cancellationToken);
        return result.Succeeded
            ? Ok()
            : BadRequest(result);
    }

    /// <summary>Resets a password.</summary>
    [HttpPost("resetPassword")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType<IdentityResultResponse>(400)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto dto,
        CancellationToken cancellationToken)
    {
        IdentityResultResponse result = await mediator.Send(
            new ResetPasswordRequest(
                dto.Email,
                dto.ResetCode,
                dto.NewPassword),
            cancellationToken);
        return result.Succeeded
            ? Ok()
            : BadRequest(result);
    }

    /// <summary>Gets authenticated identity information.</summary>
    [HttpGet("manage/info")]
    [Authorize]
    [ProducesResponseType<IdentityInfoResponse>(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType<ResponseBase<IdentityInfoResponse>>(404)]
    public async Task<IActionResult> GetInfo(
        CancellationToken cancellationToken)
    {
        string? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        ResponseBase<IdentityInfoResponse> result =
            await mediator.Send(
                new GetIdentityInfoRequest(userId),
                cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : NotFound(result);
    }

    /// <summary>Updates authenticated identity information.</summary>
    [HttpPost("manage/info")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType<IdentityResultResponse>(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateInfo(
        [FromBody] InfoDto dto,
        CancellationToken cancellationToken)
    {
        string? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        IdentityResultResponse result = await mediator.Send(
            new UpdateIdentityInfoRequest(
                userId,
                dto.NewEmail,
                dto.NewPassword,
                dto.OldPassword),
            cancellationToken);
        return result.Succeeded
            ? Ok()
            : BadRequest(result);
    }

    /// <summary>Configures two-factor authentication.</summary>
    [HttpPost("manage/2fa")]
    [Authorize]
    [ProducesResponseType<TwoFactorResponse>(200)]
    [ProducesResponseType<IdentityResultResponse>(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ConfigureTwoFactor(
        [FromBody] TwoFactorDto dto,
        CancellationToken cancellationToken)
    {
        string? userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        ResponseBase<TwoFactorResponse> result =
            await mediator.Send(
                new ConfigureTwoFactorRequest(
                    userId,
                    dto.Enable,
                    dto.TwoFactorCode,
                    dto.ResetRecoveryCodes,
                    dto.ResetSharedKey,
                    dto.ForgetMachine),
                cancellationToken);
        return result.Success
            ? Ok(result.Value)
            : BadRequest(result);
    }

    /// <summary>Lists all roles in the catalog with hierarchy rank.</summary>
    [HttpGet("roles")]
    [Authorize(Policy = AppPermissions.RolesRead)]
    [ProducesResponseType<ResponseBase<IReadOnlyList<RoleCatalogResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListCatalogRoles(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListCatalogRolesRequest(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets the assigned roles for a target user.</summary>
    [HttpGet("users/{userId}/roles")]
    [Authorize(Policy = AppPermissions.RolesRead)]
    [ProducesResponseType<ResponseBase<UserRolesResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<UserRolesResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserRoles(
        [FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserRolesRequest(userId), cancellationToken);
        return result.Success
            ? Ok(result)
            : NotFound(result);
    }

    /// <summary>Assigns a catalog role to a target user, strictly enforcing hierarchy and anti-self-elevation.</summary>
    [HttpPost("users/{userId}/roles")]
    [Authorize(Policy = AppPermissions.RolesManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        [FromRoute] string userId,
        [FromBody] AssignRoleDto dto,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new AssignUserRoleRequest(actorUserId, userId, dto.RoleName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Removes a role from a target user, strictly enforcing hierarchy, anti-self-elevation, and last-admin protection.</summary>
    [HttpDelete("users/{userId}/roles/{roleName}")]
    [Authorize(Policy = AppPermissions.RolesManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(
        [FromRoute] string userId,
        [FromRoute] string roleName,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new RemoveUserRoleRequest(actorUserId, userId, roleName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Lists all permissions and named permission groups in the catalog.</summary>
    [HttpGet("permissions")]
    [Authorize(Policy = AppPermissions.PermissionsRead)]
    [ProducesResponseType<ResponseBase<PermissionCatalogResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListCatalogPermissions(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListCatalogPermissionsRequest(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets effective permissions, direct grants, explicit revocations, and groups for a target user.</summary>
    [HttpGet("users/{userId}/permissions")]
    [Authorize(Policy = AppPermissions.PermissionsRead)]
    [ProducesResponseType<ResponseBase<UserPermissionsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ResponseBase<UserPermissionsResponse>>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissions(
        [FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPermissionsRequest(userId), cancellationToken);
        return result.Success
            ? Ok(result)
            : NotFound(result);
    }

    /// <summary>Grants a permission directly to a user.</summary>
    [HttpPost("users/{userId}/permissions/grant")]
    [Authorize(Policy = AppPermissions.PermissionsManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GrantPermission(
        [FromRoute] string userId,
        [FromBody] GrantPermissionDto dto,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new GrantUserPermissionRequest(actorUserId, userId, dto.PermissionName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Explicitly revokes a permission from a user (overriding roles and groups).</summary>
    [HttpPost("users/{userId}/permissions/revoke")]
    [Authorize(Policy = AppPermissions.PermissionsManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokePermission(
        [FromRoute] string userId,
        [FromBody] RevokePermissionDto dto,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new RevokeUserPermissionRequest(actorUserId, userId, dto.PermissionName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Clears any direct grant or explicit revocation override for a permission on a user.</summary>
    [HttpDelete("users/{userId}/permissions/{permissionName}")]
    [Authorize(Policy = AppPermissions.PermissionsManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearPermission(
        [FromRoute] string userId,
        [FromRoute] string permissionName,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new ClearUserPermissionRequest(actorUserId, userId, permissionName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Assigns a user to a named permission group.</summary>
    [HttpPost("users/{userId}/groups")]
    [Authorize(Policy = AppPermissions.PermissionsManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignGroup(
        [FromRoute] string userId,
        [FromBody] AssignPermissionGroupDto dto,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new AssignUserPermissionGroupRequest(actorUserId, userId, dto.GroupName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    /// <summary>Removes a user from a named permission group.</summary>
    [HttpDelete("users/{userId}/groups/{groupName}")]
    [Authorize(Policy = AppPermissions.PermissionsManage)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseBase<bool>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveGroup(
        [FromRoute] string userId,
        [FromRoute] string groupName,
        CancellationToken cancellationToken)
    {
        string? actorUserId = GetUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        var result = await mediator.Send(
            new RemoveUserPermissionGroupRequest(actorUserId, userId, groupName),
            cancellationToken);

        if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result);
        }

        return result.Success
            ? Ok(result)
            : BadRequest(result);
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
